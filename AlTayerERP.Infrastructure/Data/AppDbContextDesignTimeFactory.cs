using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlTayerERP.Infrastructure.Data;

/// <summary>
/// يوفر سياق EF لأدوات الترحيل فقط. لا يستخدم إعدادات التشغيل ولا يفتح اتصالاً بقاعدة بيانات.
/// </summary>
public sealed class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>();
        options.UseMySql(
            "Server=127.0.0.1;Port=3306;Database=altayer_ef_design;User=design_time;Password=not_used;",
            new MySqlServerVersion(new Version(8, 0, 0)));

        return new AppDbContext(options.Options);
    }
}
