using System.ComponentModel.DataAnnotations;
using MyBuyingList.Application.Common.Constants;
using MyBuyingList.Domain.ValueObjects;

namespace MyBuyingList.Application.Common.Validations;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ValidEmailAttribute : ValidationAttribute
{
    public ValidEmailAttribute() : base(ValidationMessages.InvalidEmail) { }

    public override bool IsValid(object? value)
        => value is string email && EmailAddress.IsValid(email);
}
