using Microsoft.AspNetCore.Mvc;
using MyBuyingList.Application.Features.Users.DTOs;
using MyBuyingList.Application.Features.Users.Services;
using MyBuyingList.Domain.Constants;
using MyBuyingList.Web.Middlewares.Authorization;

namespace MyBuyingList.Web.Pages.Users;

[HasPermission(Policies.UserDelete)]
public class DeleteModel(IUserService userService) : BasePageModel
{
    public UserDto? TargetUser { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        TargetUser = await userService.GetUserAsync(id, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken)
    {
        await userService.DeleteAsync(id, cancellationToken);
        return RedirectToPage("/Users/Index");
    }
}
