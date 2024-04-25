using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;
using System.Text;
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
        _ = services.AddHttpClient("ZD", client =>
        {
            client.BaseAddress = new("https://nationsbenefits.zendesk.com/api/v2/");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"sankalp.godugu@nationsbenefits.com/token:2T6Xd5Vpw6dwmKvUo8XZj1nPK4o5td9qheoSaER3")));
        });
        _ = services.AddTransient<IZendeskClientService, ZendeskClientService>((s) =>
        {
            IHttpClientFactory httpClientFactory = s.GetRequiredService<IHttpClientFactory>();
            IConfiguration configuration = s.GetRequiredService<IConfiguration>();
            return new ZendeskClientService(httpClientFactory, configuration);
        });
    })
    .Build();
host.Run();
