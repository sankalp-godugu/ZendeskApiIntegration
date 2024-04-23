using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZendeskApiIntegration.App.Interfaces;
using ZendeskApiIntegration.DataLayer.Interfaces;
using ZendeskApiIntegration.TriggerUtilities;

namespace ZendeskApiIntegration
{
    public class Functions(IDataLayer dataLayer, IConfiguration config, IZendeskClientService zendeskClientService, ILogger<Functions> log)
    {
        //[Function("BulkCreateGroupMemberships")]
        //public async Task<IActionResult> BulkCreateGroupMemberships([TimerTrigger("*/5 * * * *", RunOnStartup = true, UseMonitor = true)] TimerInfo timer)
        //{
        //    log.LogInformation("C# HTTP trigger function processed a request.");
        //    _ = await ZendeskApiUtilities.ProcessZendeskTask(dataLayer, config, zendeskClientService, log);
        //    return new OkObjectResult("Welcome to Azure Functions!");
        //}

        //[Function("BulkDeleteTickets")]
        //public async Task<IActionResult> BulkDeleteTickets([TimerTrigger("*/5 * * * *", RunOnStartup = true, UseMonitor = true)] TimerInfo timer)
        //{
        //    log.LogInformation("C# HTTP trigger function processed a request.");
        //    _ = await ZendeskApiUtilities.ProcessZendeskTask(dataLayer, config, zendeskClientService, log);
        //    return new OkObjectResult("Welcome to Azure Functions!");
        //}

        //[Function("BulkMoveTickets")]
        //public async Task<IActionResult> BulkMoveTickets([TimerTrigger("*/5 * * * *", RunOnStartup = true, UseMonitor = true)] TimerInfo timer)
        //{
        //    log.LogInformation("C# HTTP trigger function processed a request.");
        //    _ = await ZendeskApiUtilities.ProcessZendeskTask(dataLayer, config, zendeskClientService, log);
        //    return new OkObjectResult("Welcome to Azure Functions!");
        //}

        //[Function("GetTicketsWithIncorrectAddress")]
        //public async Task<IActionResult> GetTicketsWithIncorrectAddress([TimerTrigger("*/5 * * * *", RunOnStartup = true, UseMonitor = true)] TimerInfo timer)
        //{
        //    log.LogInformation("C# HTTP trigger function processed a request.");
        //    _ = await ZendeskApiUtilities.ProcessZendeskTask(dataLayer, config, zendeskClientService, log);
        //    return new OkObjectResult("Welcome to Azure Functions!");
        //}

        [Function("BulkSuspendUsers")]
        public async Task<IActionResult> BulkSuspendUsers([TimerTrigger("*/5 * * * *", RunOnStartup = true, UseMonitor = true)] TimerInfo timer)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            _ = await ZendeskApiUtilities.ProcessBulkSuspendUseres(dataLayer, config, zendeskClientService, log);
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
