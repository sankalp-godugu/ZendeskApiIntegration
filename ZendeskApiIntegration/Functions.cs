using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZendeskApiIntegration.App.Interfaces;
using ZendeskApiIntegration.DataLayer.Interfaces;
using ZendeskApiIntegration.TriggerUtilities;
using ActivityTriggerAttribute = Microsoft.Azure.Functions.Worker.ActivityTriggerAttribute;
using DurableClientAttribute = Microsoft.Azure.Functions.Worker.DurableClientAttribute;
using OrchestrationTriggerAttribute = Microsoft.Azure.Functions.Worker.OrchestrationTriggerAttribute;

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

        #region End User Suspension Process

        /*[Function(nameof(Run))]
        public static async Task Run(
        [TimerTrigger("0 0 9 * * WED", RunOnStartup = true)] TimerInfo timer,
        [DurableClient] DurableTaskClient starter,
        FunctionContext context)
        {
            var log = context.GetLogger("TimerTriggeredStarter");
            log.LogInformation($"Timer trigger function executed at: {DateTime.Now}");

            // Start the orchestration
            string instanceId = await starter.ScheduleNewOrchestrationInstanceAsync(nameof(RunOrchestrator),StartOrchestrationOptions.);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        }*/

        [Function(nameof(RunOrchestrator))]
        public static async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            await context.CallActivityAsync(nameof(NotifyInactiveEndUsers), null);

            var deadline =
                //context.CurrentUtcDateTime.AddDays(7).AddHours(8);
                context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(5));
            await context.CreateTimer(deadline, CancellationToken.None);

            await context.CallActivityAsync(nameof(SuspendInactiveEndUsers), null);
        }

        [Function(nameof(NotifyInactiveEndUsers))]
        //public async Task<IActionResult> NotifyInactiveEndUsers([ActivityTrigger] object input)
        public async Task<IActionResult> NotifyInactiveEndUsers([TimerTrigger("0 0 9 * * WED", RunOnStartup = true, UseMonitor = true)] TimerInfo timer)
        {
            _ = await ZendeskApiUtilities.NotifyEndUsers(dataLayer, config, zendeskClientService, log);
            return new OkObjectResult("Welcome to Azure Functions!");
        }

        [Function(nameof(SuspendInactiveEndUsers))]
        public async Task<IActionResult> SuspendInactiveEndUsers([ActivityTrigger] object input)
        //public async Task<IActionResult> SuspendInactiveEndUsers([TimerTrigger("*/5 * * * *", RunOnStartup = true, UseMonitor = true)] TimerInfo timer)
        {
            _ = await ZendeskApiUtilities.NotifyClientServices(dataLayer, config, zendeskClientService, null, log);
            return new OkObjectResult("Welcome to Azure Functions!");
        }

        #endregion
    }
}
