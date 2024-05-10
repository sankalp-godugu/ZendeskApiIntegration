using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZendeskApiIntegration.App.Interfaces;
using ZendeskApiIntegration.DataLayer.Interfaces;
using ZendeskApiIntegration.Model;
using static ZendeskApiIntegration.App.Services.ZendeskClientService;
using static ZendeskApiIntegration.Utilities.Constants;

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
        public static async Task<IActionResult> NotifyAndSuspendInactiveEndUsers(IDataLayer dataLayer, IConfiguration config, IZendeskClientService zendeskClientService, ILogger log)
        {
            try
            {
                await Task.Run(async () =>
                {
                    string appConnectionString = config["DataBase:APPConnectionString"] ?? Environment.GetEnvironmentVariable("DataBase:ConnectionString");

                    log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
                    log?.LogInformation("********* Member PD Orders => Zendesk End User Suspension Automation **********");

                    Filter filter = new()
                    {
                        LastLoginAt = DateTime.Now.AddMonths(-1).Date.ToString("yyyy-MM-dd"),
                        OrgId = Organizations.Nations,
                        Role = Roles.EndUser,
                        Type = Types.User
                    };
                    List<User> inactiveEndUsers = await zendeskClientService.GetInactiveUsers(filter, log);
                    List<User> endUsersNotifiedLastWeek = zendeskClientService.GetEndUsersFromLastReport(inactiveEndUsers, log);
                    List<User> inactiveEndUsersToSuspend = inactiveEndUsers.Intersect(endUsersNotifiedLastWeek, new UserEmailEqualityComparer()).ToList();
                    //inactiveEndUsersToSuspend = GetTestUsers();

                    await zendeskClientService.GetUserOrganizations(inactiveEndUsersToSuspend, log);
                    //ShowJobStatusResponse suspendUsersResponse = await zendeskClientService.SuspendUsers(true, inactiveEndUsersToSuspend, log);
                    int notifyClientServicesResponse = await zendeskClientService.NotifyClientServices(inactiveEndUsersToSuspend, log);

                    List<User> endUsersToNotify = inactiveEndUsers.Except(inactiveEndUsersToSuspend).ToList();
                    bool hasUsersToNotify = endUsersToNotify.Count > 0;
                    endUsersToNotify = hasUsersToNotify ? endUsersToNotify : [];
                    await zendeskClientService.GetUserOrganizations(endUsersToNotify, log);
                    
                    int sendEmailMultipleResult = await zendeskClientService.NotifyEndUsers(endUsersToNotify, log);

                    log?.LogInformation("********* Member PD Orders => Zendesk End User Suspension Automation Finished **********");
                });

                return new OkObjectResult("Task of processing PD Orders in Zendesk has been allocated to azure function and see logs for more information about its progress...");
            }
            catch (Exception ex)
            {
                log?.LogError($"Failed with an exception with message: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public static async Task<IActionResult?> ProcessBulkGroupAssignment(IDataLayer dataLayer, IConfiguration config, IZendeskClientService zendeskClientService, ILogger log)
        {
            try
            {
                await Task.Run(async () =>
                {
                    log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
                    log?.LogInformation("********* Member PD Orders => Zendesk Bulk Group Assignment **********");

                    await zendeskClientService.CreateGroupMemberships(log);

                    return;
                });

                return null;
            }
            catch (Exception ex)
            {
                log?.LogError($"Failed with an exception with message: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        private static List<User> GetTestUsers()
        {
            return [
                new() { Id = 18139432493847, Email = Emails.MyEmail, Name = Users.MyName, OrganizationId = Organizations.Nations, Suspended = false },
                new User() { Id = 19641229464983, Email = Emails.EmailTestAustinPersonal, Name = Users.TestNameAustin, OrganizationId = Organizations.Nations, Suspended = false },
                new() { Id = 19613889634711, Email = Emails.EmailTestJudson, Name = Users.TestNameJudson, OrganizationId = Organizations.Nations, Suspended = false }
                //new() { Id = 19539794011543, Email = Emails.EmailTestJudson2, Name = Users.TestNameJudson, OrganizationId = Organizations.Nations, Suspended = false },
                //new() { Id = 17793394708887, Email = Emails.EmailNationsAustinStephens, Name = Users.TestNameAustin, OrganizationId = Organizations.Nations, Suspended = false },
            ];
        }

        private static List<User> GetUsersFailedToSendEmail()
        {
            return [
                new() { Id = 19868705669527, Email = "cspencer@txihp.com", Name = "Caroline Spencer", OrganizationId = 19868705669527, Suspended = false },
                new() { Id = 19944633851415, Email = "kris.karl@cdphp.com", Name = "Kris Karl", OrganizationId = 19944633851415, Suspended = false },
                new() { Id = 20153110099095, Email = "kelly.staudt@communityhealthchoice.org", Name = "Kelly Staudt", OrganizationId = 20153110099095, Suspended = false },
                new() { Id = 19501638260631, Email = "mcecchi@brighthealthcare.com", Name = "Martte Cecchi", OrganizationId = 20153110099095, Suspended = false }
            ];
        }
    }
}