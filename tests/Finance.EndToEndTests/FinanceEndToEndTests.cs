using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Finance.EndToEndTests;

public class FinanceEndToEndTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public FinanceEndToEndTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AppHealthEndpoints_ShouldRespondSuccessfully()
    {
        var response = await _client.GetAsync("/health");
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized);
    }
}
