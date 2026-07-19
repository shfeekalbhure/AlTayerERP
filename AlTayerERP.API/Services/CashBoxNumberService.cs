using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services
{
    public class CashBoxNumberService
    {
        private readonly AppDbContext _context;

        public CashBoxNumberService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateCashBoxCodeAsync(string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                throw new Exception("رقم الشركة مطلوب.");

            companyId = companyId.Trim();

            var codes = await _context.Cash_Boxes
                .AsNoTracking()
                .Where(x => x.Company_ID == companyId)
                .Select(x => x.CashBox_Code)
                .ToListAsync();

            int maxNumber = 0;

            foreach (var code in codes)
            {
                if (string.IsNullOrWhiteSpace(code))
                    continue;

                string numericPart = code
                    .Replace("CB-", "")
                    .Replace("CB", "")
                    .Trim();

                if (int.TryParse(numericPart, out int number) &&
                    number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            int nextNumber = maxNumber + 1;

            return $"CB-{nextNumber:D5}";
        }
    }
}