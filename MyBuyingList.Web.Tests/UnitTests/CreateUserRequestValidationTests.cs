using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using MyBuyingList.Application.Common.Constants;
using MyBuyingList.Application.Features.Users.DTOs;
using MyBuyingList.Domain.Constants;

namespace MyBuyingList.Web.Tests.UnitTests;

public class CreateUserRequestValidationTests
{
    private const string ValidPassword = "Password123%";
    private const string ValidEmail = "test@gmail.com";
    private const string ValidUsername = "sample_username";

    private static List<ValidationResult> Validate(CreateUserRequest input)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);
        return results;
    }

    private static string UsernameLengthError =>
        $"The field UserName must be a string with a minimum length of {FieldLengths.USER_USERNAME_MIN_LENGTH} and a maximum length of {FieldLengths.USER_USERNAME_MAX_LENGTH}.";

    private static string EmailLengthError =>
        $"The field Email must be a string with a maximum length of {FieldLengths.USER_EMAIL_MAX_LENGTH}.";

    private static CreateUserRequest CreateDto(string username, string email, string password)
    {
        return new CreateUserRequest()
        {
            UserName = username,
            Email = email,
            Password = password
        };
    }
    
    public static IEnumerable<object[]> ValidCreateUserRequests()
    {
        var fixture = new Fixture();

        // Testing username
        yield return [CreateDto("som", ValidEmail, ValidPassword)]; // length 3
        yield return [CreateDto("something_aleatory_wlength_32", ValidEmail, ValidPassword)]; // length 32
        yield return [CreateDto("NuMbeR_1234567890", ValidEmail, ValidPassword)]; // all numbers
        yield return [CreateDto("NuMbeR_____1234567890", ValidEmail, ValidPassword)]; // multiple underscores

        // Testing email
        yield return [CreateDto(ValidUsername, "validemail@gmail.com", ValidPassword)];
        yield return [CreateDto(ValidUsername, "V@BB.UE", ValidPassword)];
        yield return [CreateDto(ValidUsername, fixture.Create<MailAddress>().Address, ValidPassword)]; // length 3
        yield return [CreateDto(ValidUsername, "stuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz@example.com", ValidPassword)
        ]; // length 254

        // Passwords are tested on the PasswordValidatorTests class
    }

    public static IEnumerable<object[]> InvalidCreateUserRequests()
    {
        // Invalid usernames
        yield return
        [
            CreateDto("so", ValidEmail, ValidPassword),
            UsernameLengthError
        ];

        yield return
        [
            CreateDto("something_aleatory_greater_than_length_32", ValidEmail, ValidPassword),
            UsernameLengthError
        ];

        yield return
        [
            CreateDto("", ValidEmail, ValidPassword),
            UsernameLengthError
        ];

        yield return
        [
            CreateDto("invalid!@#$", ValidEmail, ValidPassword),
            ValidationMessages.InvalidUsername
        ];

        // Invalid email
        yield return
        [
            CreateDto(ValidUsername, "", ValidPassword),
            ValidationMessages.InvalidEmail
        ];

        yield return
        [
            CreateDto(ValidUsername, "astuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz@example.com", ValidPassword),
            EmailLengthError
        ];

        yield return
        [
            CreateDto(ValidUsername, "a##@gmail.com", ValidPassword),
            ValidationMessages.InvalidEmail
        ];

        yield return
        [
            CreateDto(ValidUsername, "abc@.com", ValidPassword),
            ValidationMessages.InvalidEmail
        ];

        yield return
        [
            CreateDto(ValidUsername, "abc@gmail.", ValidPassword),
            ValidationMessages.InvalidEmail
        ];

        yield return
        [
            CreateDto(ValidUsername, "abc@gmail", ValidPassword),
            ValidationMessages.InvalidEmail
        ];

        // Invalid password
        yield return
        [
            CreateDto(ValidUsername, ValidEmail, "123"),
            ValidationMessages.InvalidPassword
        ];
    }

    [Theory]
    [MemberData(nameof(ValidCreateUserRequests))]
    public void Validate_ShouldReturnSuccess_WhenThereAreNoErrors(CreateUserRequest dto)
    {
        // Act
        List<ValidationResult> results = Validate(dto);

        // Assert
        results.Where(r => r.ErrorMessage is not null).Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(InvalidCreateUserRequests))]
    public void Validate_ShouldReturnError_WhenThereAreErrors(CreateUserRequest dto, string errorMessage)
    {
        // Act
        List<ValidationResult> results = Validate(dto);

        // Assert
        results.First().ErrorMessage.Should().Be(errorMessage);
    }
    
    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        CreateUserRequest input = new() { UserName = ValidUsername, Email = ValidEmail, Password = ValidPassword };

        Validate(input).Should().BeEmpty();
    }

    // Username

    [Theory]
    [InlineData("so")]                                              // 2 chars — below min
    [InlineData("something_aleatory_greater_than_length_32_abc")]  // above max
    public void Validate_ShouldFail_WithLengthMessage_WhenUsernameViolatesLengthConstraints(string username)
    {
        CreateUserRequest input = new() { UserName = username, Email = ValidEmail, Password = ValidPassword };

        List<ValidationResult> results = Validate(input);

        results.Should().NotBeEmpty();
        results.First().ErrorMessage.Should().Be(UsernameLengthError);
    }

    [Theory]
    [InlineData("invalid!@#$")]
    [InlineData("has space")]
    [InlineData("has-hyphen")]
    public void Validate_ShouldFail_WithInvalidUsernameMessage_WhenUsernameContainsDisallowedCharacters(string username)
    {
        CreateUserRequest input = new() { UserName = username, Email = ValidEmail, Password = ValidPassword };

        List<ValidationResult> results = Validate(input);

        results.Should().NotBeEmpty();
        results.First().ErrorMessage.Should().Be(ValidationMessages.InvalidUsername);
    }
}
