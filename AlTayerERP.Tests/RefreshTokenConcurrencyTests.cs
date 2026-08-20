using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class RefreshTokenConcurrencyTests
{
    [Fact]
    public async Task ConditionalRefreshConsumption_AllowsOnlyOneIndependentRequest()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"altayer-refresh-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={databasePath};Default Timeout=30;Pooling=False";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connectionString)
            .Options;

        try
        {
            await using (var setup = new AppDbContext(options))
            {
                await setup.Database.EnsureCreatedAsync();
                setup.Refresh_Tokens.Add(new RefreshToken
                {
                    Token_Hash = "hash-used-by-two-requests",
                    Session_ID = "session-1",
                    User_ID = 42,
                    Company_ID = "ALTAYER",
                    Branch_ID = 1,
                    Fiscal_Year_ID = 2026,
                    Device_ID = "device-1",
                    Expires_At = DateTime.UtcNow.AddMinutes(30)
                });
                await setup.SaveChangesAsync();
            }

            var first = ConsumeAsync(options, "hash-used-by-two-requests");
            var second = ConsumeAsync(options, "hash-used-by-two-requests");
            var affectedRows = await Task.WhenAll(first, second);

            Assert.Equal(1, affectedRows.Sum());

            await using var verification = new AppDbContext(options);
            var stored = await verification.Refresh_Tokens.SingleAsync();
            Assert.NotNull(stored.Revoked_At);
            Assert.Equal("ROTATED", stored.Revoked_Reason);
        }
        finally
        {
            if (File.Exists(databasePath))
                File.Delete(databasePath);
        }
    }

    private static async Task<int> ConsumeAsync(DbContextOptions<AppDbContext> options, string tokenHash)
    {
        await using var context = new AppDbContext(options);
        return await context.Refresh_Tokens
            .Where(x => x.Token_Hash == tokenHash && x.Revoked_At == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Revoked_At, DateTime.UtcNow)
                .SetProperty(x => x.Revoked_Reason, "ROTATED"));
    }
}
