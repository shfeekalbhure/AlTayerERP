using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace AlTayerERP.API.Controllers
{
    /// <summary>إدارة تصنيفات دليل الحسابات المعزولة حسب شركة الجلسة.</summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AccountCategoriesController : ControllerBase
    {
        private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
            { "Asset", "Liability", "Equity", "Revenue", "Expense" };

        private readonly AppDbContext _context;
        private readonly ScreenAuthorizationService _authorization;
        private readonly AuditTrailService _audit;

        public AccountCategoriesController(
            AppDbContext context,
            ScreenAuthorizationService authorization,
            AuditTrailService audit)
        {
            _context = context;
            _authorization = authorization;
            _audit = audit;
        }

        private ServerSession? Session => HttpContext.Items["ServerSession"] as ServerSession;

        private async Task<IActionResult?> RequireAsync(ScreenOperation operation)
        {
            if (Session == null)
                return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });

            return await _authorization.IsAllowedAsync(Session, "ChartOfAccounts", operation)
                ? null
                : Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? accountType, [FromQuery] bool activeOnly = false)
        {
            var error = await RequireAsync(ScreenOperation.View);
            if (error != null || Session == null) return error!;

            accountType = NormalizeType(accountType);
            if (!string.IsNullOrEmpty(accountType) && !AllowedTypes.Contains(accountType))
                return BadRequest(new { message = "نوع الحساب غير معتمد." });

            var rows = new List<object>();
            await using DbConnection connection = _context.Database.GetDbConnection();
            await EnsureOpenAsync(connection);

            await using DbCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT Category_ID, Category_Code, Category_Name_AR, Category_Name_EN,
       Account_Type, Normal_Balance, Is_System, Is_Active, Sort_Order
FROM account_categories
WHERE Company_ID = @company
  AND (@type = '' OR Account_Type = @type)
  AND (@activeOnly = 0 OR Is_Active = 1)
ORDER BY Account_Type, Sort_Order, Category_Name_AR;";
            AddParameter(command, "@company", Session.Company_ID);
            AddParameter(command, "@type", accountType ?? string.Empty);
            AddParameter(command, "@activeOnly", activeOnly ? 1 : 0);

            await using DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new
                {
                    Category_ID = reader.GetString(0),
                    Category_Code = reader.GetString(1),
                    Category_Name_AR = reader.GetString(2),
                    Category_Name_EN = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Account_Type = reader.GetString(4),
                    Normal_Balance = reader.GetString(5),
                    Is_System = reader.GetBoolean(6),
                    Is_Active = reader.GetBoolean(7),
                    Sort_Order = reader.GetInt32(8)
                });
            }

            return Ok(rows);
        }

        [HttpGet("Lookup")]
        public async Task<IActionResult> Lookup([FromQuery] string accountType)
        {
            var error = await RequireAsync(ScreenOperation.View);
            if (error != null || Session == null) return error!;

            accountType = NormalizeType(accountType) ?? string.Empty;
            if (!AllowedTypes.Contains(accountType))
                return BadRequest(new { message = "نوع الحساب غير معتمد." });

            var rows = new List<object>();
            await using DbConnection connection = _context.Database.GetDbConnection();
            await EnsureOpenAsync(connection);

            await using DbCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT Category_Code, Category_Name_AR, Category_Name_EN, Normal_Balance
FROM account_categories
WHERE Company_ID = @company AND Account_Type = @type AND Is_Active = 1
ORDER BY Sort_Order, Category_Name_AR;";
            AddParameter(command, "@company", Session.Company_ID);
            AddParameter(command, "@type", accountType);

            await using DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new
                {
                    Category_Code = reader.GetString(0),
                    Category_Name_AR = reader.GetString(1),
                    Category_Name_EN = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Normal_Balance = reader.GetString(3)
                });
            }

            return Ok(rows);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AccountCategoryDto dto)
        {
            var error = await RequireAsync(ScreenOperation.Add);
            if (error != null || Session == null) return error!;

            Normalize(dto);
            string? validation = Validate(dto);
            if (validation != null) return BadRequest(new { message = validation });

            await using DbConnection connection = _context.Database.GetDbConnection();
            await EnsureOpenAsync(connection);

            if (await ExistsAsync(connection, dto.Category_Code, null))
                return Conflict(new { message = "كود التصنيف مستخدم مسبقاً في الشركة الحالية." });

            string id = Guid.NewGuid().ToString();
            string normalBalance = DefaultBalance(dto.Account_Type);
            await using DbCommand command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO account_categories
(Category_ID, Company_ID, Category_Code, Category_Name_AR, Category_Name_EN,
 Account_Type, Normal_Balance, Is_System, Is_Active, Sort_Order, Created_At, Created_By)
VALUES
(@id, @company, @code, @nameAr, @nameEn, @type, @balance, 0, @active, @sort, UTC_TIMESTAMP(6), @user);";
            AddParameter(command, "@id", id);
            AddParameter(command, "@company", Session.Company_ID);
            AddParameter(command, "@code", dto.Category_Code);
            AddParameter(command, "@nameAr", dto.Category_Name_AR);
            AddParameter(command, "@nameEn", (object?)dto.Category_Name_EN ?? DBNull.Value);
            AddParameter(command, "@type", dto.Account_Type);
            AddParameter(command, "@balance", normalBalance);
            AddParameter(command, "@active", dto.Is_Active ? 1 : 0);
            AddParameter(command, "@sort", dto.Sort_Order);
            AddParameter(command, "@user", Session.User_ID.ToString());
            await command.ExecuteNonQueryAsync();

            _audit.Add(Session, HttpContext, "account_categories", id, "CREATE", null,
                new { dto.Category_Code, dto.Category_Name_AR, dto.Account_Type, Normal_Balance = normalBalance, dto.Is_Active });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { accountType = dto.Account_Type }, new { Category_ID = id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] AccountCategoryDto dto)
        {
            var error = await RequireAsync(ScreenOperation.Edit);
            if (error != null || Session == null) return error!;

            Normalize(dto);
            string? validation = Validate(dto);
            if (validation != null) return BadRequest(new { message = validation });

            await using DbConnection connection = _context.Database.GetDbConnection();
            await EnsureOpenAsync(connection);

            var current = await ReadOneAsync(connection, id);
            if (current == null) return NotFound(new { message = "التصنيف غير موجود ضمن الشركة الحالية." });
            if (current.Value.IsSystem && !string.Equals(current.Value.Code, dto.Category_Code, StringComparison.Ordinal))
                return BadRequest(new { message = "لا يمكن تغيير كود تصنيف نظامي." });
            if (await ExistsAsync(connection, dto.Category_Code, id))
                return Conflict(new { message = "كود التصنيف مستخدم مسبقاً في الشركة الحالية." });

            bool used = await IsUsedAsync(connection, current.Value.Code);
            if (used && (!string.Equals(current.Value.Code, dto.Category_Code, StringComparison.Ordinal) ||
                         !string.Equals(current.Value.Type, dto.Account_Type, StringComparison.OrdinalIgnoreCase)))
                return BadRequest(new { message = "لا يمكن تغيير كود أو نوع تصنيف مستخدم في دليل الحسابات." });

            string normalBalance = DefaultBalance(dto.Account_Type);
            await using DbCommand command = connection.CreateCommand();
            command.CommandText = @"
UPDATE account_categories
SET Category_Code = @code,
    Category_Name_AR = @nameAr,
    Category_Name_EN = @nameEn,
    Account_Type = @type,
    Normal_Balance = @balance,
    Is_Active = @active,
    Sort_Order = @sort,
    Updated_At = UTC_TIMESTAMP(6),
    Updated_By = @user
WHERE Category_ID = @id AND Company_ID = @company;";
            AddParameter(command, "@code", dto.Category_Code);
            AddParameter(command, "@nameAr", dto.Category_Name_AR);
            AddParameter(command, "@nameEn", (object?)dto.Category_Name_EN ?? DBNull.Value);
            AddParameter(command, "@type", dto.Account_Type);
            AddParameter(command, "@balance", normalBalance);
            AddParameter(command, "@active", dto.Is_Active ? 1 : 0);
            AddParameter(command, "@sort", dto.Sort_Order);
            AddParameter(command, "@user", Session.User_ID.ToString());
            AddParameter(command, "@id", id);
            AddParameter(command, "@company", Session.Company_ID);
            await command.ExecuteNonQueryAsync();

            _audit.Add(Session, HttpContext, "account_categories", id, "UPDATE",
                new { current.Value.Code, current.Value.NameAr, current.Value.Type },
                new { dto.Category_Code, dto.Category_Name_AR, dto.Account_Type, Normal_Balance = normalBalance, dto.Is_Active });
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم تعديل التصنيف بنجاح." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(string id, [FromQuery] string? reason)
        {
            var error = await RequireAsync(ScreenOperation.Delete);
            if (error != null || Session == null) return error!;

            await using DbConnection connection = _context.Database.GetDbConnection();
            await EnsureOpenAsync(connection);
            var current = await ReadOneAsync(connection, id);
            if (current == null) return NotFound(new { message = "التصنيف غير موجود ضمن الشركة الحالية." });
            if (current.Value.IsSystem)
                return BadRequest(new { message = "لا يمكن إيقاف تصنيف نظامي." });
            if (await IsUsedAsync(connection, current.Value.Code))
                return BadRequest(new { message = "لا يمكن إيقاف تصنيف مستخدم في دليل الحسابات." });

            await using DbCommand command = connection.CreateCommand();
            command.CommandText = @"
UPDATE account_categories
SET Is_Active = 0, Updated_At = UTC_TIMESTAMP(6), Updated_By = @user
WHERE Category_ID = @id AND Company_ID = @company;";
            AddParameter(command, "@user", Session.User_ID.ToString());
            AddParameter(command, "@id", id);
            AddParameter(command, "@company", Session.Company_ID);
            await command.ExecuteNonQueryAsync();

            _audit.Add(Session, HttpContext, "account_categories", id, "DEACTIVATE",
                new { Is_Active = true }, new { Is_Active = false }, reason);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إيقاف التصنيف بنجاح." });
        }

        private async Task<bool> ExistsAsync(DbConnection connection, string code, string? exceptId)
        {
            await using DbCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT COUNT(*) FROM account_categories
WHERE Company_ID = @company AND Category_Code = @code
  AND (@exceptId = '' OR Category_ID <> @exceptId);";
            AddParameter(command, "@company", Session!.Company_ID);
            AddParameter(command, "@code", code);
            AddParameter(command, "@exceptId", exceptId ?? string.Empty);
            return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        }

        private async Task<bool> IsUsedAsync(DbConnection connection, string code)
        {
            await using DbCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT COUNT(*) FROM chart_of_accounts
WHERE Company_ID = @company AND Account_Category = @code;";
            AddParameter(command, "@company", Session!.Company_ID);
            AddParameter(command, "@code", code);
            return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        }

        private async Task<(string Code, string NameAr, string Type, bool IsSystem)?> ReadOneAsync(DbConnection connection, string id)
        {
            await using DbCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT Category_Code, Category_Name_AR, Account_Type, Is_System
FROM account_categories
WHERE Category_ID = @id AND Company_ID = @company;";
            AddParameter(command, "@id", id);
            AddParameter(command, "@company", Session!.Company_ID);
            await using DbDataReader reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;
            return (reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3));
        }

        private static async Task EnsureOpenAsync(DbConnection connection)
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();
        }

        private static void AddParameter(DbCommand command, string name, object value)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        private static string? NormalizeType(string? value) => value?.Trim() switch
        {
            "أصل" => "Asset",
            "خصم" or "خصوم" => "Liability",
            "حقوق ملكية" => "Equity",
            "إيراد" => "Revenue",
            "مصروف" => "Expense",
            var type => string.IsNullOrWhiteSpace(type) ? null : type
        };

        private static void Normalize(AccountCategoryDto dto)
        {
            dto.Category_Code = dto.Category_Code?.Trim() ?? string.Empty;
            dto.Category_Name_AR = dto.Category_Name_AR?.Trim() ?? string.Empty;
            dto.Category_Name_EN = string.IsNullOrWhiteSpace(dto.Category_Name_EN) ? null : dto.Category_Name_EN.Trim();
            dto.Account_Type = NormalizeType(dto.Account_Type) ?? string.Empty;
        }

        private static string? Validate(AccountCategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Category_Code) ||
                string.IsNullOrWhiteSpace(dto.Category_Name_AR) ||
                string.IsNullOrWhiteSpace(dto.Account_Type))
                return "كود التصنيف والاسم العربي ونوع الحساب حقول مطلوبة.";
            if (!AllowedTypes.Contains(dto.Account_Type))
                return "نوع الحساب غير معتمد.";
            if (!dto.Category_Code.All(c => char.IsLetterOrDigit(c) || c == '_'))
                return "كود التصنيف يقبل الحروف والأرقام والشرطة السفلية فقط.";
            return null;
        }

        private static string DefaultBalance(string accountType) =>
            accountType is "Asset" or "Expense" ? "Debit" : "Credit";
    }
}
