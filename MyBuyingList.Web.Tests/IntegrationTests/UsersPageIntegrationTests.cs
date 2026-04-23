using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyBuyingList.Application.Common.Interfaces;
using MyBuyingList.Domain.Entities;
using MyBuyingList.Infrastructure;
using MyBuyingList.Web.Tests.IntegrationTests.Common;
using System.Net;

namespace MyBuyingList.Web.Tests.IntegrationTests;

public class UsersPageIntegrationTests : BaseIntegrationTest
{
    private readonly HttpClient _client;
    private readonly ResourceFactory _factory;
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    public UsersPageIntegrationTests(ResourceFactory resourceFactory)
        : base(resourceFactory)
    {
        _client = resourceFactory.HttpClient;
        _factory = resourceFactory;
    }

    private async Task<HttpResponseMessage> PostFormAsync(
        HttpClient client, string path, Dictionary<string, string> extraFields,
        CancellationToken cancellationToken)
    {
        string html = await (await client.GetAsync(path, cancellationToken)).Content.ReadAsStringAsync(cancellationToken);
        string token = Utils.ExtractAntiForgeryToken(html);

        Dictionary<string, string> form = new(extraFields)
        {
            ["__RequestVerificationToken"] = token
        };

        return await client.PostAsync(path, new FormUrlEncodedContent(form), cancellationToken);
    }

    [Fact]
    public async Task UsersIndex_ReturnsOk_WhenAdminIsAuthenticated()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync(Constants.AddressUsersPage, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UsersIndex_RedirectsToLogin_WhenUnauthenticated()
    {
        // Arrange
        HttpClient anonymousClient = _factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync(Constants.AddressUsersPage, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/login");
    }

    [Fact]
    public async Task UsersIndex_ReturnsForbidden_WhenUserLacksPermission()
    {
        // Arrange — test user has no permissions
        await _factory.InsertTestUserAsync();
        HttpClient testClient = await _factory.CreateAuthenticatedClientAsync(
            Utils.TestUserUsername, Utils.TestUserPassword);

        // Act
        HttpResponseMessage response = await testClient.GetAsync(Constants.AddressUsersPage, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/access-denied");
    }

    [Fact]
    public async Task CreateUser_RedirectsToUsers_WhenFormIsValid()
    {
        // Act
        HttpResponseMessage response = await PostFormAsync(_client, Constants.AddressUsersCreatePage, new()
        {
            ["Input.Username"] = "newuser123",
            ["Input.Email"] = "newuser@example.com",
            ["Input.Password"] = "ValidPass1!"
        }, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Be(Constants.AddressUsersPage);
    }

    [Fact]
    public async Task ChangePassword_ReturnsOk_WhenAdminChangesOwnPassword()
    {
        // Arrange
        int adminUserId = 2; // integration_admin seeded as second user
        string path = $"/users/{adminUserId}/change-password";

        // Act
        HttpResponseMessage response = await PostFormAsync(_client, path, new()
        {
            ["Input.OldPassword"] = Utils.IntegrationTestAdminPassword,
            ["Input.NewPassword"] = "NewValid1!"
        }, _cancellationToken);

        // Assert — success redirects to /users
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Be(Constants.AddressUsersPage);
    }

    [Fact]
    public async Task ChangePassword_ReturnsForbidden_WhenNonAdminChangesAnotherUsersPassword()
    {
        // Arrange — insert test user and a second user to act as a different target
        await _factory.InsertTestUserAsync();
        HttpClient testClient = await _factory.CreateAuthenticatedClientAsync(
            Utils.TestUserUsername, Utils.TestUserPassword);

        // Act — test user (id != 1) tries to change admin's (id = 1) password
        HttpResponseMessage response = await testClient.GetAsync(
            "/users/1/change-password", _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/access-denied");
    }

    [Fact]
    public async Task ChangePassword_RedirectsToError_WhenUserDoesNotExist()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync(
            "/users/99999/change-password", _cancellationToken);

        // Assert — ResourceNotFoundException is caught by FormattedExceptionPageFilter → /error
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/error");
    }

    [Fact]
    public async Task DeleteUser_RedirectsToUsers_WhenAdminDeletesExistingUser()
    {
        // Arrange
        int testUserId = await _factory.InsertTestUserAsync();
        string path = $"/users/{testUserId}/delete";

        // Act
        HttpResponseMessage response = await PostFormAsync(_client, path, new(), _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Be(Constants.AddressUsersPage);
    }

    [Fact]
    public async Task DeleteUser_ReturnsPageWithError_WhenDeletingAdminUser()
    {
        // Arrange — admin user is seeded with id=1
        string path = "/users/1/delete";

        // Act
        HttpResponseMessage response = await PostFormAsync(_client, path, new(), _cancellationToken);

        // Assert — re-renders the page with error
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string html = await response.Content.ReadAsStringAsync(_cancellationToken);
        html.Should().Contain("admin");
    }

    [Fact]
    public async Task DeleteUser_ReturnsForbidden_WhenNonAdminDeletesAnotherUser()
    {
        // Arrange
        await _factory.InsertTestUserAsync();
        HttpClient testClient = await _factory.CreateAuthenticatedClientAsync(
            Utils.TestUserUsername, Utils.TestUserPassword);

        // Act — test user tries to delete admin (id=1)
        HttpResponseMessage response = await testClient.GetAsync("/users/1/delete", _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/access-denied");
    }

    [Fact]
    public async Task DeleteUser_RedirectsToError_WhenUserDoesNotExist()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/users/99999/delete", _cancellationToken);

        // Assert — ResourceNotFoundException is caught by FormattedExceptionPageFilter → /error
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/error");
    }

    [Fact]
    public async Task CreateUser_PersistsUserToDatabase_WhenFormIsValid()
    {
        // Act
        await PostFormAsync(_client, Constants.AddressUsersCreatePage, new()
        {
            ["Input.Username"] = "newuser123",
            ["Input.Email"] = "newuser@example.com",
            ["Input.Password"] = "ValidPass1!"
        }, _cancellationToken);

        // Assert — user exists in the DB with correct fields and a valid password hash
        using IServiceScope scope = _factory.Services.CreateScope();
        ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IPasswordEncryptionService passwordService = scope.ServiceProvider.GetRequiredService<IPasswordEncryptionService>();

        User? user = await db.Set<User>().SingleOrDefaultAsync(u => u.UserName == "newuser123", _cancellationToken);

        user.Should().NotBeNull();
        user!.Email.Should().Be("newuser@example.com");
        user.Active.Should().BeTrue();
        passwordService.VerifyPassword("ValidPass1!", user.Password).Should().BeTrue();
    }

    [Fact]
    public async Task ChangePassword_UpdatesPasswordHashInDatabase_WhenSuccessful()
    {
        // Arrange
        const string newPassword = "NewValid1!";
        int adminUserId = 2; // integration_admin seeded as second user
        string path = $"/users/{adminUserId}/change-password";

        // Act
        await PostFormAsync(_client, path, new()
        {
            ["Input.OldPassword"] = Utils.IntegrationTestAdminPassword,
            ["Input.NewPassword"] = newPassword
        }, _cancellationToken);

        // Assert — password hash in DB verifies against the new password, not the old one
        using IServiceScope scope = _factory.Services.CreateScope();
        ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IPasswordEncryptionService passwordService = scope.ServiceProvider.GetRequiredService<IPasswordEncryptionService>();

        User user = await db.Set<User>().SingleAsync(u => u.UserName == Utils.IntegrationTestAdminUsername, _cancellationToken);

        passwordService.VerifyPassword(newPassword, user.Password).Should().BeTrue();
        passwordService.VerifyPassword(Utils.IntegrationTestAdminPassword, user.Password).Should().BeFalse();
    }

    [Fact]
    public async Task ChangePassword_RedirectsToError_WhenUserIdDoesNotExist()
    {
        // Act — POST change-password for a non-existent user; service calls GetAsync → ResourceNotFoundException
        HttpResponseMessage response = await PostFormAsync(_client, "/users/99999/change-password", new()
        {
            ["Input.OldPassword"] = "OldValid1!",
            ["Input.NewPassword"] = "NewValid1!"
        }, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/error");
    }

    [Fact]
    public async Task UsersIndex_ReturnsSecondPage_WhenPageTwoIsRequested()
    {
        // Arrange — fill page 1 (page size = 50, 2 already seeded → need 48 more),
        // then add one extra user that lands exclusively on page 2.
        int pageSize = _factory.Configuration.GetValue<int>("RepositorySettings:PageSize");
        const string secondPageUsername = "page_two_user";

        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            IPasswordEncryptionService passwordService = scope.ServiceProvider.GetRequiredService<IPasswordEncryptionService>();
            string hash = passwordService.HashPassword("ValidPass1!");

            // Fill the remainder of page 1 alongside the 2 seeded users
            int usersNeededToFillPageOne = pageSize - 2;
            List<User> fillUsers = Enumerable.Range(0, usersNeededToFillPageOne).Select(i => new User
            {
                UserName = $"fill_user_{i:D3}",
                Email = $"fill_user_{i}@test.local",
                Password = hash,
                Active = true
            }).ToList();
            db.Set<User>().AddRange(fillUsers);
            await db.SaveChangesAsync(_cancellationToken);

            // This user gets the next ID and lands on page 2
            db.Set<User>().Add(new User
            {
                UserName = secondPageUsername,
                Email = "page_two_user@test.local",
                Password = hash,
                Active = true
            });
            await db.SaveChangesAsync(_cancellationToken);
        }

        // Act
        HttpResponseMessage response = await _client.GetAsync(
            $"{Constants.AddressUsersPage}?page=2", _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string html = await response.Content.ReadAsStringAsync(_cancellationToken);
        html.Should().Contain(secondPageUsername);
    }

    [Fact]
    public async Task DeleteUser_SetsUserInactiveInDatabase_WhenSuccessful()
    {
        // Arrange
        int testUserId = await _factory.InsertTestUserAsync();
        string path = $"/users/{testUserId}/delete";

        // Act
        await PostFormAsync(_client, path, new(), _cancellationToken);

        // Assert — soft delete: record remains in DB with Active = false
        using IServiceScope scope = _factory.Services.CreateScope();
        ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        User? user = await db.Set<User>().SingleOrDefaultAsync(u => u.Id == testUserId, _cancellationToken);

        user.Should().NotBeNull();
        user!.Active.Should().BeFalse();
    }
}
