using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MyBuyingList.Application.Common.Validations;
using MyBuyingList.Application.Features.Users.Services;
using MyBuyingList.Domain.Constants;
using MyBuyingList.Web.Middlewares.Authorization;

namespace MyBuyingList.Web.Pages.Users;

public class ChangePasswordInputModel
{
    [BindRequired]
    [ValidPassword]
    public required string OldPassword { get; init; }

    [BindRequired]
    [ValidPassword]
    public required string NewPassword { get; init; }
}

[HasPermission(Policies.UserUpdate)]
public class ChangePasswordModel(IUserService userService) : BasePageModel
{
    [BindProperty]
    public ChangePasswordInputModel Input { get; set; } = null!;
    
    public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken)
    {
        await userService.ChangeUserPasswordAsync(id, Input.OldPassword, Input.NewPassword, cancellationToken);
        return RedirectToPage("/Users/Index");
    }
}
