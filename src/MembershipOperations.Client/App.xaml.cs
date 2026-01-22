using MembershipOperations.Client.Core;
using MembershipOperations.Client.Services;
using MembershipOperations.Client.ViewModels;
using MembershipOperations.Client.Views;
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
                var basePath = AppContext.BaseDirectory;

                config.SetBasePath(basePath);

                System.Diagnostics.Debug.WriteLine($"Config base path: {basePath}");
                System.Diagnostics.Debug.WriteLine($"Config file exists: {File.Exists(Path.Combine(basePath, "appsettings.json"))}");

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

                services.AddTransient<LoginViewModel>();
                services.AddTransient<LoginWindow>();
            })
            .Build();

        base.OnStartup(e);

        // Show login
        var login = _host.Services.GetRequiredService<LoginWindow>();
        var ok = login.ShowDialog();

        if (ok != true)
        {
            Shutdown();
            return;
        }

        // TEMP: until MainWindow exists
        MessageBox.Show("Login succeeded. Next: open MainWindow.");
        Shutdown();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
            await _host.StopAsync();

        base.OnExit(e);
    }
}
