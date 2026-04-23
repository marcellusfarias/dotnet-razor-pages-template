using System.Text.RegularExpressions;

namespace MyBuyingList.Web.Tests.IntegrationTests.Common;

public static class Utils
{
    public const string TestUserEmail = "test_user@test.local";
    public const string TestUserPassword = "Mx485!@zz";
    public const string TestUserUsername = "test_user";

    public const string IntegrationTestAdminUsername = "integration_admin";
    public const string IntegrationTestAdminPassword = "IntAdmin!Test1";

    public static async Task LoginAsync(HttpClient client, string username, string password)
    {
        string html = await (await client.GetAsync(Constants.AddressLoginPage)).Content.ReadAsStringAsync();
        string antiForgeryToken = ExtractAntiForgeryToken(html);

        Dictionary<string, string> formData = new()
        {
            ["Input.Username"] = username,
            ["Input.Password"] = password,
            ["__RequestVerificationToken"] = antiForgeryToken
        };

        await client.PostAsync(Constants.AddressLoginPage, new FormUrlEncodedContent(formData));
    }

    public static string ExtractAntiForgeryToken(string html)
    {
        Match match = Regex.Match(html, @"name=""__RequestVerificationToken""[^>]+value=""([^""]+)""");
        if (!match.Success)
            match = Regex.Match(html, @"value=""([^""]+)""[^>]+name=""__RequestVerificationToken""");

        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}
