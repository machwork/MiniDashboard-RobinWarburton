using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using MiniDashboard.App.Services;
using MiniDashboard.App.ViewModels;

namespace MiniDashboard.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        // Register HttpClient and ApiService
        // If running locally without the API, you can swap to MockApiService for development by setting the "UseMockApi" environment variable.
        var useMock = Environment.GetEnvironmentVariable("USE_MOCK_API") == "true";
        if (useMock)
        {
            services.AddSingleton<IApiService, MockApiService>();
        }
        else
        {
            services.AddHttpClient<IApiService, ApiService>((provider, client) =>
            {
                client.BaseAddress = new Uri("https://localhost:7044/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            
            // Configure ApiService with cache service
            services.AddTransient<IApiService>(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(nameof(IApiService));
                var logger = provider.GetRequiredService<ILogger<ApiService>>();
                var cacheService = provider.GetService<ICacheService>();
                return new ApiService(httpClient, logger, cacheService);
            });
        }

        // Register Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ICacheService, CacheService>();
        
        // Register ViewModels
        services.AddTransient<MainViewModel>();

        // Register MainWindow
        services.AddTransient<MainWindow>(provider =>
        {
            var viewModel = provider.GetRequiredService<MainViewModel>();
            return new MainWindow(viewModel);
        });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

