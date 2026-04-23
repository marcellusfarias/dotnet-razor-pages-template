using System.ComponentModel.DataAnnotations;

namespace MyBuyingList.Web.Configuration;

public class CookieAuthOptions
{
    public const string SectionName = "CookieAuthOptions";

    [Range(1, int.MaxValue)]
    public int ExpirationMinutes { get; init; } = 60;

    public bool SlidingExpiration { get; init; } = true;
}
