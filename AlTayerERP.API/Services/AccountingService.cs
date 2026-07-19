using AlTayerERP.Infrastructure.Data;

namespace AlTayerERP.API.Services
{
    // ======================================================
    // محرك المحاسبة المركزي
    // مسؤول لاحقًا عن إنشاء القيود المحاسبية
    // من البوالص، التذاكر، سندات القبض، سندات الصرف
    // ======================================================
    public class AccountingService
    {
        // الاتصال بقاعدة البيانات
        private readonly AppDbContext _context;

        // Constructor
        public AccountingService(AppDbContext context)
        {
            _context = context;
        }
    }
}