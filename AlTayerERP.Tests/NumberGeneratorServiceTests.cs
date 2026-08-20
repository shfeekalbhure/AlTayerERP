using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class NumberGeneratorServiceTests
{
    [Fact]
    public async Task ReserveNextNumberAsync_RejectsMissingDocumentType()
    {
        await using var connection = await OpenDatabaseAsync();
        await using var context = CreateContext(connection);
        var service = new NumberGeneratorService(context);

        var exception = await Assert.ThrowsAsync<NumberingException>(
            () => service.ReserveNextNumberAsync(" ", null, null, null));

        Assert.Contains("نوع المستند مطلوب", exception.Message);
    }

    [Fact]
    public async Task ReserveNextNumberAsync_UsesSequentialCounterWithinItsScope()
    {
        await using var connection = await OpenDatabaseAsync();
        await using var context = CreateContext(connection);
        context.Numbering_Settings.Add(new NumberingSetting
        {
            Document_Type = "TEST_VOUCHER",
            Prefix = "TV",
            Digits_Count = 3,
            Reset_Type = "NEVER",
            Use_Company = false,
            Use_Branch = false,
            Use_Year = false,
            Is_Active = true
        });
        await context.SaveChangesAsync();

        var service = new NumberGeneratorService(context);
        var first = await service.ReserveNextNumberAsync("test_voucher", null, null, null);
        var second = await service.ReserveNextNumberAsync("TEST_VOUCHER", null, null, null);

        Assert.Equal("TV-001", first.Document_Number);
        Assert.Equal("TV-002", second.Document_Number);
        Assert.Equal(1, first.Serial_Number);
        Assert.Equal(2, second.Serial_Number);
    }

    private static async Task<SqliteConnection> OpenDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var setup = CreateContext(connection);
        await setup.Database.EnsureCreatedAsync();
        return connection;
    }

    private static AppDbContext CreateContext(SqliteConnection connection) => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options);
}
