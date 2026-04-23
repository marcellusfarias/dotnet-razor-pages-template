using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyBuyingList.Domain.Entities;
using MyBuyingList.Infrastructure;
using MyBuyingList.Web.Tests.IntegrationTests.Common;
using System.Net;

namespace MyBuyingList.Web.Tests.IntegrationTests;

public class LoginPageIntegrationTests : BaseIntegrationTest
{
    private readonly ResourceFactory _factory;
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    public LoginPageIntegrationTests(ResourceFactory resourceFactory)
        : base(resourceFactory)
    {
        _factory = resourceFactory;
    }

    private HttpClient CreateFreshClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static async Task<HttpResponseMessage> PostLoginAsync(
        HttpClient client, string username, string password, CancellationToken cancellationToken)
    {
        string html = await (await client.GetAsync(Constants.AddressLoginPage, cancellationToken)).Content.ReadAsStringAsync(cancellationToken);
        string token = Utils.ExtractAntiForgeryToken(html);

        Dictionary<string, string> form = new()
        {
            ["Input.Username"] = username,
            ["Input.Password"] = password,
            ["__RequestVerificationToken"] = token
        };

        return await client.PostAsync(Constants.AddressLoginPage, new FormUrlEncodedContent(form), cancellationToken);
    }

    [Fact]
    public async Task Login_RedirectsToUsers_WhenCredentialsAreValid()
    {
        // Arrange
        await _factory.InsertTestUserAsync();
        HttpClient client = CreateFreshClient();

        // Act
        HttpResponseMessage response = await PostLoginAsync(client, Utils.TestUserUsername, Utils.TestUserPassword, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Be(Constants.AddressUsersPage);
    }

    [Fact]
    public async Task Login_SetsAuthCookie_WhenCredentialsAreValid()
    {
        // Arrange
        await _factory.InsertTestUserAsync();
        HttpClient client = CreateFreshClient();

        // Act
        HttpResponseMessage loginResponse = await PostLoginAsync(
            client, Utils.TestUserUsername, Utils.TestUserPassword, _cancellationToken);

        // Assert — login succeeded (302 redirect to /users)
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        loginResponse.Headers.Location?.ToString().Should().Be(Constants.AddressUsersPage);

        // Cookie is stored: /users redirects to /access-denied (not /login) because
        // the user is authenticated but lacks the UserGetAll permission.
        HttpResponseMessage usersResponse = await client.GetAsync(Constants.AddressUsersPage, _cancellationToken);
        usersResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        usersResponse.Headers.Location?.ToString().Should().Contain("/access-denied");
    }

    [Fact]
    public async Task Login_ReturnsPageWithError_WhenCredentialsAreInvalid()
    {
        // Arrange
        await _factory.InsertTestUserAsync();
        HttpClient client = CreateFreshClient();

        // Act
        HttpResponseMessage response = await PostLoginAsync(
            client, Utils.TestUserUsername, Utils.TestUserPassword + ".", _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string html = await response.Content.ReadAsStringAsync(_cancellationToken);
        html.Should().Contain("Invalid username or password");
    }

    [Fact]
    public async Task Login_ReturnsPageWithLockoutMessage_WhenAccountIsLocked()
    {
        // Arrange — exhaust MaxFailedAttempts (3 in test config)
        await _factory.InsertTestUserAsync();
        HttpClient client = CreateFreshClient();
        string wrongPassword = Utils.TestUserPassword + ".";

        for (int i = 0; i < 3; i++)
            await PostLoginAsync(client, Utils.TestUserUsername, wrongPassword, _cancellationToken);

        // Act
        HttpResponseMessage response = await PostLoginAsync(client, Utils.TestUserUsername, wrongPassword, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string html = await response.Content.ReadAsStringAsync(_cancellationToken);
        html.Should().Contain("temporarily locked");
    }

    [Fact]
    public async Task Login_ReturnsPageWithLockoutMessage_WhenLockedEvenWithCorrectPassword()
    {
        // Arrange
        await _factory.InsertTestUserAsync();
        HttpClient client = CreateFreshClient();
        string wrongPassword = Utils.TestUserPassword + ".";

        for (int i = 0; i < 3; i++)
            await PostLoginAsync(client, Utils.TestUserUsername, wrongPassword, _cancellationToken);

        // Act
        HttpResponseMessage response = await PostLoginAsync(client, Utils.TestUserUsername, Utils.TestUserPassword, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string html = await response.Content.ReadAsStringAsync(_cancellationToken);
        html.Should().Contain("temporarily locked");
    }

    [Fact]
    public async Task Login_RedirectsToValidReturnUrl_WhenReturnUrlIsLocal()
    {
        // Arrange
        await _factory.InsertTestUserAsync();
        HttpClient client = CreateFreshClient();

        string html = await (await client.GetAsync(Constants.AddressLoginPage, _cancellationToken)).Content.ReadAsStringAsync(_cancellationToken);
        string token = Utils.ExtractAntiForgeryToken(html);

        Dictionary<string, string> form = new()
        {
            ["Input.Username"] = Utils.TestUserUsername,
            ["Input.Password"] = Utils.TestUserPassword,
            ["ReturnUrl"] = "/users/create",
            ["__RequestVerificationToken"] = token
        };

        // Act
        HttpResponseMessage response = await client.PostAsync(
            Constants.AddressLoginPage, new FormUrlEncodedContent(form), _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Be("/users/create");
    }

    [Fact]
    public async Task Login_RedirectsToUsers_WhenReturnUrlIsExternal()
    {
        // Arrange
        await _factory.InsertTestUserAsync();
        HttpClient client = CreateFreshClient();

        string html = await (await client.GetAsync(Constants.AddressLoginPage, _cancellationToken)).Content.ReadAsStringAsync(_cancellationToken);
        string token = Utils.ExtractAntiForgeryToken(html);

        Dictionary<string, string> form = new()
        {
            ["Input.Username"] = Utils.TestUserUsername,
            ["Input.Password"] = Utils.TestUserPassword,
            ["ReturnUrl"] = "https://evil.example.com/steal",
            ["__RequestVerificationToken"] = token
        };

        // Act
        HttpResponseMessage response = await client.PostAsync(
            Constants.AddressLoginPage, new FormUrlEncodedContent(form), _cancellationToken);

        // Assert — external URL rejected, redirects to default
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Be(Constants.AddressUsersPage);
    }

    [Fact]
    public async Task Logout_ClearsAuthCookie_AndRedirectsToLogin()
    {
        // Arrange — use a fresh authenticated client to avoid corrupting the shared HttpClient
        HttpClient client = await _factory.CreateAuthenticatedClientAsync(
            Utils.IntegrationTestAdminUsername, Utils.IntegrationTestAdminPassword);

        string html = await (await client.GetAsync(Constants.AddressUsersPage, _cancellationToken)).Content.ReadAsStringAsync(_cancellationToken);
        string token = Utils.ExtractAntiForgeryToken(html);

        Dictionary<string, string> form = new()
        {
            ["__RequestVerificationToken"] = token
        };

        // Act
        HttpResponseMessage logoutResponse = await client.PostAsync(
            Constants.AddressLogoutPage, new FormUrlEncodedContent(form), _cancellationToken);

        // Assert — redirect to login
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        logoutResponse.Headers.Location?.ToString().Should().Contain("/login");

        // After logout, /users should redirect to login (unauthenticated)
        HttpResponseMessage usersResponse = await client.GetAsync(Constants.AddressUsersPage, _cancellationToken);
        usersResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        usersResponse.Headers.Location?.ToString().Should().Contain("/login");
    }

    [Fact]
    public async Task Login_RedirectsToUserPages_WhenLoginAfterLockoutExpires()
    {
        // Arrange — insert user, exhaust failed attempts to trigger lockout
        await _factory.InsertTestUserAsync();
        HttpClient client = CreateFreshClient();
        string wrongPassword = Utils.TestUserPassword + ".";

        for (int i = 0; i < 3; i++)
            await PostLoginAsync(client, Utils.TestUserUsername, wrongPassword, _cancellationToken);

        // Simulate lockout expiry by backdating LockoutEnd in the DB
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            User user = await db.Set<User>().SingleAsync(u => u.UserName == Utils.TestUserUsername, _cancellationToken);
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(-1);
            await db.SaveChangesAsync(_cancellationToken);
        }

        // Act — log in with correct credentials after lockout has expired
        HttpResponseMessage response = await PostLoginAsync(
            client, Utils.TestUserUsername, Utils.TestUserPassword, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Be(Constants.AddressUsersPage);
    }

    [Fact]
    public async Task Login_RateLimits_WhenExcessiveRequestsAreMade()
    {
        // PermitLimit=10 in test settings. Use a fresh client to avoid interference from
        // other tests and verify the policy is wired correctly by hammering past the limit.
        HttpClient client = CreateFreshClient();

        string html = await (await client.GetAsync(Constants.AddressLoginPage, _cancellationToken)).Content.ReadAsStringAsync(_cancellationToken);
        string token = Utils.ExtractAntiForgeryToken(html);

        Dictionary<string, string> form = new()
        {
            ["Input.Username"] = "any",
            ["Input.Password"] = "any",
            ["__RequestVerificationToken"] = token
        };

        HttpResponseMessage? lastResponse = null;
        for (int i = 0; i <= 10; i++)
        {
            lastResponse = await client.PostAsync(
                Constants.AddressLoginPage, new FormUrlEncodedContent(form), _cancellationToken);
            if (lastResponse.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                break;
        }

        lastResponse!.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        // Wait for the rate limit window to reset so subsequent tests get a fresh budget.
        // All TestServer clients share IPAddress.Loopback as the partition key.
        await Task.Delay(1100, _cancellationToken);
    }
}
