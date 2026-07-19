using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services
{
    public class CostCenterNumberService
    {
        private readonly AppDbContext _context;

        public CostCenterNumberService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateNextCodeAsync(
            string companyId,
            string? parentCostCenterId)
        {
            companyId = (companyId ?? "").Trim();

            if (string.IsNullOrWhiteSpace(companyId))
                throw new Exception("رقم الشركة مطلوب.");

            // المستوى الأول: 1، 2، 3...
            if (string.IsNullOrWhiteSpace(parentCostCenterId))
            {
                var rootCodes = await _context.Cost_Centers
                    .AsNoTracking()
                    .Where(x =>
                        x.Company_ID == companyId &&
                        x.Parent_Cost_Center_ID == null)
                    .Select(x => x.Center_Code)
                    .ToListAsync();

                int maxRoot = 0;

                foreach (string? code in rootCodes)
                {
                    if (int.TryParse(code, out int number) &&
                        number > maxRoot)
                    {
                        maxRoot = number;
                    }
                }

                return (maxRoot + 1).ToString();
            }

            var parent = await _context.Cost_Centers
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Cost_Center_ID == parentCostCenterId &&
                    x.Company_ID == companyId);

            if (parent == null)
                throw new Exception("مركز التكلفة الأب غير موجود.");

            var childCodes = await _context.Cost_Centers
                .AsNoTracking()
                .Where(x =>
                    x.Company_ID == companyId &&
                    x.Parent_Cost_Center_ID == parentCostCenterId)
                .Select(x => x.Center_Code)
                .ToListAsync();

            string parentCode = parent.Center_Code.Trim();
            int maxSerial = 0;

            // أبناء المستوى الأول: 11، 12، 13...
            if (parent.Center_Level == 1)
            {
                foreach (string? code in childCodes)
                {
                    if (string.IsNullOrWhiteSpace(code) ||
                        !code.StartsWith(parentCode))
                        continue;

                    string suffix = code.Substring(parentCode.Length);

                    // نأخذ رقمًا واحدًا فقط
                    if (suffix.Length == 1 &&
                        int.TryParse(suffix, out int serial) &&
                        serial > maxSerial)
                    {
                        maxSerial = serial;
                    }
                }

                return parentCode + (maxSerial + 1);
            }

            // أبناء المستوى الثاني وما بعده:
            // 11001، 11002، 12001...
            foreach (string? code in childCodes)
            {
                if (string.IsNullOrWhiteSpace(code) ||
                    !code.StartsWith(parentCode))
                    continue;

                string suffix = code.Substring(parentCode.Length);

                if (suffix.Length == 3 &&
                    int.TryParse(suffix, out int serial) &&
                    serial > maxSerial)
                {
                    maxSerial = serial;
                }
            }

            return parentCode + (maxSerial + 1).ToString("D3");
        }
    }
}