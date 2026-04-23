using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MyBuyingList.Web.Services;

namespace MyBuyingList.Web.Tests.UnitTests.Services;

public class CurrentUserServiceTests
{
    private static IHttpContextAccessor BuildAccessor(string? nameIdValue)
    {
        Claim[] claims = nameIdValue is null
            ? []
            : [new Claim(ClaimTypes.NameIdentifier, nameIdValue)];

        ClaimsIdentity identity = new(claims, "Test");
        ClaimsPrincipal principal = new(identity);
        DefaultHttpContext context = new() { User = principal };

        IHttpContextAccessor accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);
        return accessor;
    }

    [Fact]
    public void UserId_WithValidClaim_ReturnsUserId()
    {
        IHttpContextAccessor accessor = BuildAccessor("42");
        CurrentUserService sut = new(accessor);

        sut.UserId.Should().Be(42);
    }

    [Fact]
    public void UserId_WithMissingClaim_ThrowsInvalidOperationException()
    {
        IHttpContextAccessor accessor = BuildAccessor(null);
        CurrentUserService sut = new(accessor);

        Action act = () => _ = sut.UserId;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UserId_WithNonNumericClaim_ThrowsInvalidOperationException()
    {
        IHttpContextAccessor accessor = BuildAccessor("not-a-number");
        CurrentUserService sut = new(accessor);

        Action act = () => _ = sut.UserId;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UserId_WithNullHttpContext_ThrowsInvalidOperationException()
    {
        IHttpContextAccessor accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);
        CurrentUserService sut = new(accessor);

        Action act = () => _ = sut.UserId;

        act.Should().Throw<InvalidOperationException>();
    }
}
