using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZendeskApiIntegration.App.Interfaces;
using ZendeskApiIntegration.App.Services;
using ZendeskApiIntegration.DataLayer.Interfaces;
using ZendeskApiIntegration.DataLayer.Services;
using ZendeskApiIntegration.Utilities;

IHost host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        _ = services.AddMemoryCache();
        IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
        CacheManager.Initialize(memoryCache);
        _ = services.AddApplicationInsightsTelemetryWorkerService();
        _ = services.ConfigureFunctionsApplicationInsights();
        _ = services.AddSingleton<IDataLayer, DataLayer>();
        _ = services.AddTransient<IZendeskClientService, ZendeskClientService>((s) =>
        {
            IHttpClientFactory httpClientFactory = s.GetRequiredService<IHttpClientFactory>();
            IConfiguration configuration = s.GetRequiredService<IConfiguration>();
            return new ZendeskClientService(httpClientFactory, configuration);
        });
    })
    .Build();
host.Run();
