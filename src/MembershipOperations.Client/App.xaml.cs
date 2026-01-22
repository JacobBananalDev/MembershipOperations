using MembershipOperations.Client.Core;
using MembershipOperations.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;

namespace MembershipOperations.Client;

public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<ApiOptions>(context.Configuration.GetSection("Api"));

                services.AddSingleton<AuthSession>();

                services.AddHttpClient<ApiClient>((sp, http) =>
                {
                    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiOptions>>().Value;
                    http.BaseAddress = new Uri(options.BaseUrl);
                });

                services.AddSingleton<AuthSession>();

                services.AddHttpClient<ApiClient>((sp, http) =>
                {
                    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiOptions>>().Value;
                    http.BaseAddress = new Uri(options.BaseUrl);
                });

                services.AddScoped<AuthApi>();
            })
            .Build();

        base.OnStartup(e);

        // Later: start LoginWindow via DI
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
            await _host.StopAsync();

        base.OnExit(e);
    }
}
