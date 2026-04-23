using Microsoft.AspNetCore.Mvc;
using MyBuyingList.Application.Features.Users.DTOs;
using MyBuyingList.Application.Features.Users.Services;
using MyBuyingList.Domain.Constants;
using MyBuyingList.Web.Middlewares.Authorization;

namespace MyBuyingList.Web.Pages.Users;

[HasPermission(Policies.UserCreate)]
public class CreateModel(IUserService userService) : BasePageModel
{
    [BindProperty]
    public CreateUserRequest Input { get; set; } = null!;

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await userService.CreateAsync(Input, cancellationToken);
        return RedirectToPage("/Users/Index");
    }
}
