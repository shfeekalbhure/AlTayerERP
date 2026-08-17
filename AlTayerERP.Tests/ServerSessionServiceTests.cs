using AlTayerERP.API.Services;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class ServerSessionServiceTests
{
    [Fact]
    public void Create_ReturnsSessionThatCanBeRetrievedByToken()
    {
        var service = new ServerSessionService();

        var created = service.Create(
            userId: 7,
            roleId: 3,
            isSystemAdmin: false,
            companyId: "CO-01",
            branchId: 2,
            yearId: 2026);

        var found = service.TryGet(created.Access_Token, out var session);

        Assert.True(found);
        Assert.Equal(created.Access_Token, session.Access_Token);
        Assert.Equal(7, session.User_ID);
        Assert.Equal(3, session.Role_ID);
        Assert.Equal("CO-01", session.Company_ID);
        Assert.Equal(2, session.Branch_ID);
        Assert.Equal(2026, session.Year_ID);
        Assert.True(session.Expires_At > DateTime.UtcNow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown-token")]
    public void TryGet_ReturnsFalseForMissingOrUnknownToken(string? token)
    {
        var service = new ServerSessionService();

        var found = service.TryGet(token, out _);

        Assert.False(found);
    }

    [Fact]
    public void Remove_InvalidatesExistingSession()
    {
        var service = new ServerSessionService();
        var created = service.Create(1, 1, true, "CO-01", 1, 2026);

        service.Remove(created.Access_Token);

        Assert.False(service.TryGet(created.Access_Token, out _));
    }
}
