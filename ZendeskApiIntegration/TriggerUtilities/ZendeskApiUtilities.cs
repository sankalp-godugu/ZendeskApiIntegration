using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZendeskApiIntegration.App.Interfaces;
using ZendeskApiIntegration.DataLayer.Interfaces;
using ZendeskApiIntegration.Model;

namespace ZendeskApiIntegration.TriggerUtilities
{
    /// <summary>
    /// Zendesk utilities.
    /// </summary>
    public static class ZendeskApiUtilities
    {
        /// <summary>
        /// Processes Zendesk contacts, performing operations such as logging, configuration retrieval,
        /// data layer interaction, and Zendesk API service calls.
        /// </summary>
        /// <param name="_logger">An instance of the <see cref="ILogger"/> interface for logging.</param>
        /// <param name="_configuration">An instance of the <see cref="IConfiguration"/> interface for accessing configuration settings.</param>
        /// <param name="_dataLayer">An instance of the <see cref="IDataLayer"/> interface or class for interacting with the data layer.</param>
        /// <param name="_ZendeskClientService">An instance of the <see cref="IZendeskClientService"/> interface or class for Zendesk API service calls.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the Zendesk contacts processing.</returns>
        public static async Task<IActionResult> ProcessBulkSuspendUseres(IDataLayer dataLayer, IConfiguration config, IZendeskClientService zendeskClientService, ILogger log)
        {
            try
            {
                await Task.Run(async () =>
                {
                    string appConnectionString = config["DataBase:APPConnectionString"] ?? Environment.GetEnvironmentVariable("DataBase:ConnectionString");

                    log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
                    log?.LogInformation("********* Member PD Orders => Zendesk Contact List Execution Started **********");

                    //await zendeskClientService.CreateGroupMemberships(log);

                    Filter filter = new()
                    {
                        LastLoginAt = DateTime.Now.AddMonths(-1).Date.ToString("yyyy-MM-dd"),
                        OrgId = 16807567180439,
                        Role = "end-user"
                    };
                    List<User> listOfUsersToSuspend = await zendeskClientService.GetInactiveUsers(filter, log);
                    /*var listOfUsersToSuspend = new List<User>
                    {
                        new() { Id = 19641229464983, Email = "ahstephens01@gmail.com", Name = "Austin Stephens", Suspended = false },
                        new() { Id = 18139432493847, Email = "sankalp.godugu@nationsbenefits.com", Name = "Sankalp Godugu", Suspended = false }
                    };*/
                    //await zendeskClientService.SendEmailMultiple(listOfUsersToSuspend, log);
                    await zendeskClientService.SendEmail(listOfUsersToSuspend, log);
                    await zendeskClientService.SuspendUsers(true, listOfUsersToSuspend, log);
                });

                return new OkObjectResult("Task of processing PD Orders in Zendesk has been allocated to azure function and see logs for more information about its progress...");
            }
            catch (Exception ex)
            {
                log?.LogError($"Failed with an exception with message: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}