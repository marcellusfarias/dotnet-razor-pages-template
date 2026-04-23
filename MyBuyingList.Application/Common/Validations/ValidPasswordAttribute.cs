using System.ComponentModel.DataAnnotations;
using MyBuyingList.Application.Common.Constants;
using MyBuyingList.Application.Common.Helpers;

namespace MyBuyingList.Application.Common.Validations;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ValidPasswordAttribute : ValidationAttribute
{
    public ValidPasswordAttribute() : base(ValidationMessages.InvalidPassword) { }

    public override bool IsValid(object? value)
        => value is string password && PasswordValidator.IsValidPassword(password);
}
