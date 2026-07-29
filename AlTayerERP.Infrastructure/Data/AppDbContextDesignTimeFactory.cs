using System.Text.Json;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MySqlConnector;

namespace AlTayerERP.Infrastructure.Data;

/// <summary>
/// مصنع التصميم يقرأ الاتصال المحلي فقط من متغير البيئة أو User Secrets لمشروع API.
/// لا يفتح اتصالًا ولا ينشئ قاعدة ولا يطبق Migration.
/// </summary>
public sealed class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string TargetDatabase = "altayer_erp_db_clean";
    private static readonly MySqlServerVersion ServerVersion = new(new Version(8, 0, 36));

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = ResolveConnectionString();
        ValidateTargetDatabase(connectionString);

        var options = new DbContextOptionsBuilder<AppDbContext>();
        options.UseMySql(connectionString, ServerVersion);

        // AppDbContext.OnModelCreating هو المسار الوحيد لتطبيق إعدادات Baseline.
        return new AppDbContext(options.Options);
    }

    private static string ResolveConnectionString()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
            return fromEnvironment;

        var fromUserSecrets = TryReadApiUserSecret();
        if (!string.IsNullOrWhiteSpace(fromUserSecrets))
            return fromUserSecrets;

        throw new InvalidOperationException(
            "لم يُعثر على اتصال التصميم. عيّن ConnectionStrings__DefaultConnection أو أضف " +
            "ConnectionStrings:DefaultConnection إلى User Secrets لمشروع AlTayerERP.API.");
    }

    private static void ValidateTargetDatabase(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);
        var database = builder.Database?.Trim();

        if (string.Equals(database, "altayer_erp_db", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "تم رفض تنفيذ EF: اتصال التصميم يشير إلى altayer_erp_db المحمية.");
        }

        if (!string.Equals(database, TargetDatabase, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"تم رفض تنفيذ EF: القاعدة المستهدفة يجب أن تكون {TargetDatabase} فقط.");
        }
    }

    private static string? TryReadApiUserSecret()
    {
        var apiProjectPath = FindApiProjectPath();
        if (apiProjectPath is null)
            return null;

        var userSecretsId = ReadUserSecretsId(apiProjectPath);
        if (string.IsNullOrWhiteSpace(userSecretsId))
            return null;

        foreach (var secretsPath in GetUserSecretsPaths(userSecretsId))
        {
            if (!File.Exists(secretsPath))
                continue;

            using var document = JsonDocument.Parse(File.ReadAllText(secretsPath));
            if (TryGetConnectionString(document.RootElement, out var connectionString))
                return connectionString;
        }

        return null;
    }

    private static string? FindApiProjectPath()
    {
        for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
        {
            var childProject = Path.Combine(directory.FullName, "AlTayerERP.API", "AlTayerERP.API.csproj");
            if (File.Exists(childProject))
                return childProject;

            var currentProject = Path.Combine(directory.FullName, "AlTayerERP.API.csproj");
            if (File.Exists(currentProject))
                return currentProject;
        }

        return null;
    }

    private static string? ReadUserSecretsId(string apiProjectPath)
    {
        var document = XDocument.Load(apiProjectPath);
        return document.Descendants("UserSecretsId").FirstOrDefault()?.Value.Trim();
    }

    private static IEnumerable<string> GetUserSecretsPaths(string userSecretsId)
    {
        var applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!string.IsNullOrWhiteSpace(applicationData))
        {
            yield return Path.Combine(applicationData, "Microsoft", "UserSecrets", userSecretsId, "secrets.json");
        }

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(userProfile))
        {
            yield return Path.Combine(userProfile, ".microsoft", "usersecrets", userSecretsId, "secrets.json");
        }
    }

    private static bool TryGetConnectionString(JsonElement root, out string? connectionString)
    {
        connectionString = null;

        if (root.TryGetProperty("ConnectionStrings:DefaultConnection", out var flattened) &&
            flattened.ValueKind == JsonValueKind.String)
        {
            connectionString = flattened.GetString();
            return !string.IsNullOrWhiteSpace(connectionString);
        }

        if (root.TryGetProperty("ConnectionStrings", out var connectionStrings) &&
            connectionStrings.ValueKind == JsonValueKind.Object &&
            connectionStrings.TryGetProperty("DefaultConnection", out var nested) &&
            nested.ValueKind == JsonValueKind.String)
        {
            connectionString = nested.GetString();
            return !string.IsNullOrWhiteSpace(connectionString);
        }

        return false;
    }
}
