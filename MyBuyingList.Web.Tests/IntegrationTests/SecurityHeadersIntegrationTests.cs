using MyBuyingList.Web.Tests.IntegrationTests.Common;
using System.Net;

namespace MyBuyingList.Web.Tests.IntegrationTests;

public class SecurityHeadersIntegrationTests : BaseIntegrationTest
{
    private readonly HttpClient _client;
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    public SecurityHeadersIntegrationTests(ResourceFactory resourceFactory)
        : base(resourceFactory)
    {
        _client = resourceFactory.HttpClient;
    }

    [Theory]
    [InlineData(Constants.AddressUsersPage)]
    [InlineData(Constants.AddressLoginPage)]
    [InlineData(Constants.AddressHealthEndpoint)]
    public async Task Response_ContainsSecurityHeaders(string path)
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync(path, _cancellationToken);

        // Assert
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.GetValues("X-Frame-Options").First().Should().Be("DENY");

        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options").First().Should().Be("nosniff");

        response.Headers.Should().ContainKey("Referrer-Policy");
        response.Headers.GetValues("Referrer-Policy").First().Should().Be("strict-origin-when-cross-origin");
    }
}
