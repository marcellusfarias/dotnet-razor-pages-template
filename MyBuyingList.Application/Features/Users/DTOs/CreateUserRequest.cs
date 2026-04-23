using System.ComponentModel.DataAnnotations;
using MyBuyingList.Application.Common.Validations;
using MyBuyingList.Domain.Constants;

namespace MyBuyingList.Application.Features.Users.DTOs;

public class CreateUserRequest
{
    [StringLength(FieldLengths.USER_USERNAME_MAX_LENGTH, MinimumLength = FieldLengths.USER_USERNAME_MIN_LENGTH)]
    [ValidUsername]
    public required string UserName { get; init; }
    
    [StringLength(FieldLengths.USER_EMAIL_MAX_LENGTH)] 
    [ValidEmail]
    public required string Email { get; init; }
    
    [ValidPassword]
    public required string Password { get; init; }
}