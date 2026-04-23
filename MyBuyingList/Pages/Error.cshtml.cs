using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyBuyingList.Web.Pages;

[AllowAnonymous]
public class ErrorModel : PageModel
{
    public void OnGet()
    {
    }
}
