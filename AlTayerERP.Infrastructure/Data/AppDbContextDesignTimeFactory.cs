using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AlTayerERP.Infrastructure.Data;

/// <summary>
/// مصنع تصميم يستخدم اتصالًا شكليًا فقط لبناء النموذج وتوليد Migration/SQL.
/// لا يفتح اتصالًا ولا ينشئ قاعدة ولا يطبق Migration.
/// </summary>
public sealed class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>();
        options.UseMySql(
            "Server=127.0.0.1;Port=3306;Database=phase1_design_only;User=design_only;Password=not_used;",
            new MySqlServerVersion(new Version(8, 0, 36)));
        options.ReplaceService<IModelCustomizer, Phase1ModelCustomizer>();
        return new AppDbContext(options.Options);
    }
}
