using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services
{
    public class AccountNumberService
    {
        private readonly AppDbContext _context;


        public AccountNumberService(AppDbContext context)
        {
            _context = context;
        }









        public async Task<string> GenerateAccountCodeAsync(string companyId, string? parentAccountId)
        {
            companyId = (companyId ?? "").Trim();
            parentAccountId = string.IsNullOrWhiteSpace(parentAccountId) ? null : parentAccountId.Trim();

            if (string.IsNullOrWhiteSpace(companyId))
                throw new Exception("رقم الشركة غير موجود.");

            int levelNo = 1;
            string parentCode = "";

            if (parentAccountId != null)
            {
                var parent = await _context.Chart_Of_Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Account_ID == parentAccountId &&
                        x.Company_ID == companyId);

                if (parent == null)
                    throw new Exception("الحساب الأب غير موجود أو لا يتبع نفس الشركة.");

                levelNo = parent.Account_Level + 1;
                parentCode = parent.Account_Code ?? "";
            }

            var setting = await _context.Account_Code_Settings
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Company_ID == companyId &&
                    x.Level_No == levelNo &&
                    x.Is_Active);

            if (setting == null)
                throw new Exception($"لا توجد إعدادات ترقيم للمستوى رقم {levelNo} للشركة [{companyId}].");

            var siblingCodes = await _context.Chart_Of_Accounts
                .AsNoTracking()
                .Where(x =>
                    x.Company_ID == companyId &&
                    x.Parent_Account_ID == parentAccountId)
                .Select(x => x.Account_Code)
                .ToListAsync();

            int maxSerial = 0;

            foreach (var code in siblingCodes)
            {
                if (string.IsNullOrWhiteSpace(code))
                    continue;

                string serialPart = code;

                if (!string.IsNullOrWhiteSpace(parentCode) && code.StartsWith(parentCode))
                    serialPart = code.Substring(parentCode.Length);

                if (int.TryParse(serialPart, out int serial))
                {
                    if (serial > maxSerial)
                        maxSerial = serial;
                }
            }
        


        int startNumber = setting.Start_Number <= 0 ? 1 : setting.Start_Number;
            int nextSerial = maxSerial == 0 ? startNumber : maxSerial + 1;

            int maxAllowed = setting.Max_Serial > 0
                ? setting.Max_Serial
                : (int)Math.Pow(10, setting.Segment_Length) - 1;

            if (nextSerial > maxAllowed)
                throw new Exception($"انتهى نطاق الترقيم للمستوى {levelNo}. الحد الأقصى {maxAllowed}.");

            string paddingChar = string.IsNullOrWhiteSpace(setting.Padding_Char)
                ? "0"
                : setting.Padding_Char.Trim();

            string newPart = nextSerial
                .ToString()
                .PadLeft(setting.Segment_Length, paddingChar[0]);

            string newCode = setting.Parent_Based
                ? parentCode + newPart
                : newPart;

            return newCode;
        }
    }
}