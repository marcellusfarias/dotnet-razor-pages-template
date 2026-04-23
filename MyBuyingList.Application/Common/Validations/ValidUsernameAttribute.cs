using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using MyBuyingList.Application.Common.Constants;

namespace MyBuyingList.Application.Common.Validations;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ValidUsernameAttribute : ValidationAttribute
{
    private static readonly Regex UsernameRegex = new("^[a-zA-Z0-9_]+$");

    public ValidUsernameAttribute() : base(ValidationMessages.InvalidUsername) { }

    public override bool IsValid(object? value)
        => value is string username && UsernameRegex.IsMatch(username);
}
