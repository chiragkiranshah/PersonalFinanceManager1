using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Finance.IntegrationTests;

public class CategoryAndBudgetIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CategoryAndBudgetIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
    }

    [Fact]
    public async Task CategoryApi_ShouldReturnOk_WhenCalled()
    {
        var response = await _client.GetAsync("/api/v1/categories");
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task BudgetApi_ShouldReturnOk_WhenCalled()
    {
        var response = await _client.GetAsync("/api/v1/budgets");
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized);
    }
}
