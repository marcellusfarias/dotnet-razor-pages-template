using Microsoft.AspNetCore.Mvc;
using MyBuyingList.Application.Common.Models;
using MyBuyingList.Application.Features.Users.DTOs;
using MyBuyingList.Application.Features.Users.Services;
using MyBuyingList.Domain.Constants;
using MyBuyingList.Web.Middlewares.Authorization;

namespace MyBuyingList.Web.Pages.Users;

[HasPermission(Policies.UserGetAll)]
public class IndexModel(IUserService userService) : BasePageModel
{
    public PagedResult<UserDto> Users { get; private set; } = null!;

    public async Task OnGetAsync([FromQuery] int page, CancellationToken cancellationToken)
    {
        int currentPage = page < 1 ? 1 : page;
        Users = await userService.GetAllUsersAsync(currentPage, cancellationToken);
    }
}
