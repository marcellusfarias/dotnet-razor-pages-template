using System.ComponentModel.DataAnnotations;
using MyBuyingList.Application.Common.Constants;
using MyBuyingList.Web.Pages.Users;

namespace MyBuyingList.Web.Tests.UnitTests;

public class ChangePasswordInputModelValidationTests
{
    private const string ValidPassword = "Password123%";

    private static List<ValidationResult> Validate(ChangePasswordInputModel input)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Validate_ShouldPass_WhenBothPasswordsAreValid()
    {
        ChangePasswordInputModel input = new() { OldPassword = ValidPassword, NewPassword = ValidPassword };

        Validate(input).Should().BeEmpty();
    }

    [Theory]
    [InlineData("invalid", ValidPassword)]
    [InlineData(ValidPassword, "invalid")]
    [InlineData("short1!", ValidPassword)]
    [InlineData(ValidPassword, "short1!")]
    public void Validate_ShouldFail_WithInvalidPasswordMessage_WhenPasswordDoesNotMeetPolicy(
        string oldPassword, string newPassword)
    {
        ChangePasswordInputModel input = new() { OldPassword = oldPassword, NewPassword = newPassword };

        List<ValidationResult> results = Validate(input);

        results.Should().NotBeEmpty();
        results.First().ErrorMessage.Should().Be(ValidationMessages.InvalidPassword);
    }
}
