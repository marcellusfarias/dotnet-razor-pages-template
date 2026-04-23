using MyBuyingList.Web.Middlewares.CorrelationId;
using MyBuyingList.Web.Tests.IntegrationTests.Common;
using MyBuyingList.Web.Tests.IntegrationTests.Common.Logging;
using System.Net;

namespace MyBuyingList.Web.Tests.IntegrationTests;

public class CorrelationIdMiddlewareIntegrationTests : BaseIntegrationTest
{
    private readonly HttpClient _client;
    private readonly ResourceFactory _factory;
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    public CorrelationIdMiddlewareIntegrationTests(ResourceFactory resourceFactory)
        : base(resourceFactory)
    {
        _client = resourceFactory.HttpClient;
        _factory = resourceFactory;
    }

    [Fact]
    public async Task Request_WithoutCorrelationIdHeader_ResponseContainsGeneratedCorrelationId()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync(Constants.AddressUsersPage, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey(CorrelationIdMiddleware.HeaderName);
        response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).First().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Request_WithCorrelationIdHeader_ResponseEchoesTheSameValue()
    {
        // Arrange
        string correlationId = "test-correlation-abc123";

        using HttpRequestMessage request = new(HttpMethod.Get, Constants.AddressUsersPage);
        request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).First().Should().Be(correlationId);
    }

    [Fact]
    public async Task Request_WithEmptyCorrelationIdHeader_ResponseContainsGeneratedId()
    {
        // Arrange
        using HttpRequestMessage request = new(HttpMethod.Get, Constants.AddressUsersPage);
        request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, string.Empty);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).First().Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("evil\ninjected-header: value")]
    [InlineData("evil\rvalue")]
    [InlineData("toolong_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task Request_WithInvalidCorrelationIdHeader_ResponseContainsGeneratedId(string invalidCorrelationId)
    {
        // Arrange
        using HttpRequestMessage request = new(HttpMethod.Get, Constants.AddressUsersPage);
        request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, invalidCorrelationId);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).First().Should().NotBe(invalidCorrelationId);
    }

    [Fact]
    public async Task Request_WithCorrelationIdHeader_CorrelationIdAppearsInLogs()
    {
        // Arrange
        string correlationId = Guid.NewGuid().ToString();
        _factory.LogCollector.Clear();

        using HttpRequestMessage request = new(HttpMethod.Get, Constants.AddressUsersPage);
        request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);

        // Act
        await _client.SendAsync(request, _cancellationToken);

        // Assert
        IReadOnlyList<FakeLogEntry> entries = _factory.LogCollector.Entries;
        entries.Count.Should().BeGreaterThan(0);
        entries.Should().AllSatisfy(e => e.Scopes.Any(s =>
            s.Key == "CorrelationId"
            && s.Value != null
            && s.Value.ToString() == correlationId));
    }
}
