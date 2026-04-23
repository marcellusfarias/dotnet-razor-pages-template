using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MyBuyingList.Infrastructure.Persistence.Seeders;

internal class AdminSettingsSetup : IConfigureOptions<AdminSettings>
{
    private readonly IConfiguration _configuration;

    public AdminSettingsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(AdminSettings options)
    {
        _configuration.GetSection(AdminSettings.SectionName).Bind(options);
    }
}
