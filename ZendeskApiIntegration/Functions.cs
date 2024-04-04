using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZendeskApiIntegration.App.Interfaces;
using ZendeskApiIntegration.DataLayer.Interfaces;
using ZendeskApiIntegration.TriggerUtilities;

namespace ZendeskApiIntegration
{
    public class Functions(IDataLayer dataLayer, IConfiguration config, IZendeskClientService zendeskClientService)
    {
        [Function("BulkCreateGroupMemberships")]
        public async Task<IActionResult> BulkCreateGroupMemberships([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            await ZendeskApiUtilities.ProcessZendeskContacts(dataLayer, config, zendeskClientService, log);
            return new OkObjectResult("Welcome to Azure Functions!");
        }

        [Function("BulkDeleteTickets")]
        public async Task<IActionResult> BulkDeleteTickets([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            await ZendeskApiUtilities.ProcessZendeskContacts(dataLayer, config, zendeskClientService, log);
            return new OkObjectResult("Welcome to Azure Functions!");
        }

        [Function("BulkMoveTickets")]
        public async Task<IActionResult> BulkMoveTickets([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            await ZendeskApiUtilities.ProcessZendeskContacts(dataLayer, config, zendeskClientService, log);
            return new OkObjectResult("Welcome to Azure Functions!");
        }

        [Function("BulkSuspendUsers")]
        public async Task<IActionResult> BulkSuspendUsers([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            await ZendeskApiUtilities.ProcessZendeskContacts(dataLayer, config, zendeskClientService, log);
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
