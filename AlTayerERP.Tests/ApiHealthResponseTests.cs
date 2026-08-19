using System.Text.Json;
using AlTayerERP.API.Contracts;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class ApiHealthResponseTests
{
    [Fact]
    public void ReadyResponse_ReportsReadyWhenApiAndDatabaseAreReady()
    {
        var response = new ApiHealthResponse("ready", "ready", "Development", "altayer_test");

        Assert.True(response.IsReady);
        Assert.Equal("Development", response.Environment);
        Assert.Equal("altayer_test", response.DatabaseName);
    }

    [Fact]
    public void ReadyResponse_IsCaseInsensitiveForReadinessMarkers()
    {
        var response = new ApiHealthResponse("READY", "Ready", "Staging", "altayer_stage");

        Assert.True(response.IsReady);
    }

    [Fact]
    public void UnavailableDatabase_DoesNotReportReady()
    {
        var response = new ApiHealthResponse("ready", "unavailable", "Production", null);

        Assert.False(response.IsReady);
        Assert.Null(response.DatabaseName);
    }

    [Fact]
    public void WebJsonContract_UsesStableCamelCaseFieldNames()
    {
        var response = new ApiHealthResponse("ready", "ready", "Development", "altayer_test");
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var document = JsonDocument.Parse(json);

        Assert.Equal("ready", document.RootElement.GetProperty("api").GetString());
        Assert.Equal("ready", document.RootElement.GetProperty("database").GetString());
        Assert.Equal("Development", document.RootElement.GetProperty("environment").GetString());
        Assert.Equal("altayer_test", document.RootElement.GetProperty("databaseName").GetString());
    }
}
