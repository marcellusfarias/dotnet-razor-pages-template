using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyBuyingList.Web.Pages;

public abstract class BasePageModel : PageModel
{
    public string? ErrorTitle { get; private set; }
    public string? ErrorDetails { get; private set; }

    internal void SetError(string title, string details)
    {
        ErrorTitle = title;
        ErrorDetails = details;
    }
}
