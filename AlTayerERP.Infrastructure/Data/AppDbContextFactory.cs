using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AlTayerERP.Infrastructure.Data;

/// <summary>
/// ينشئ AppDbContext لأدوات EF فقط، دون AutoDetect ودون فتح اتصال بقاعدة فعلية.
/// الاتصال شكلي لتوليد Migration وSQL، ولا يستخدم عند تشغيل API.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>();
        options.UseMySql(
            "Server=localhost;Port=3306;Database=phase1_baseline_design_only;User=design_only;Password=design_only;",
            new MySqlServerVersion(new Version(8, 0, 36)));
        options.ReplaceService<IModelCustomizer, Phase1ModelCustomizer>();
        return new AppDbContext(options.Options);
    }
}
