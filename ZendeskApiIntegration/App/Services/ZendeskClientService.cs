using ClosedXML.Excel;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using ZendeskApiIntegration.App.Interfaces;
using ZendeskApiIntegration.Model;
using ZendeskApiIntegration.Model.Requests;
using ZendeskApiIntegration.Model.Responses;
using ZendeskApiIntegration.Utilities;
using Filter = ZendeskApiIntegration.Model.Filter;
using User = ZendeskApiIntegration.Model.User;

namespace ZendeskApiIntegration.App.Services
{
    /// <summary>
    /// Zendesk client service.
    /// </summary>
    public class ZendeskClientService(IHttpClientFactory httpClientFactory, IConfiguration config) : IZendeskClientService
    {
        #region Public Methods

        private HttpClient GetZendeskHttpClient()
        {
            return httpClientFactory.CreateClient("ZD");
        }

        private async Task<string> GetAuthToken()
        {
            using HttpClient httpClient = GetZendeskHttpClient();
            string tokenUrl = config[ConfigConstants.TokenUrlKey] ?? Environment.GetEnvironmentVariable(ConfigConstants.TokenUrlKey);
            Uri tokenUri = new(tokenUrl);
            Dictionary<string, string> form = new()
            {
                {"grant_type", config[ConfigConstants.GrantTypeKey] ?? Environment.GetEnvironmentVariable(ConfigConstants.GrantTypeKey)},
                {"client_id", config[ConfigConstants.ClientIdKey] ?? Environment.GetEnvironmentVariable(ConfigConstants.ClientIdKey)},
                {"client_secret", config[ConfigConstants.ClientSecretKey] ?? Environment.GetEnvironmentVariable(ConfigConstants.ClientSecretKey)}
            };

            HttpResponseMessage result = await httpClient.PostAsync(tokenUri, new FormUrlEncodedContent(form));
            _ = result.EnsureSuccessStatusCode();
            string response = await result.Content.ReadAsStringAsync();
            AccessTokenResponse tokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(response);
            return tokenResponse.AccessToken;
        }

        private static void SetCacheToken(AccessTokenResponse accessTokenResponse)
        {
            //In a real-world application we should store the token in a cache service and set an TTL.
            Environment.SetEnvironmentVariable("token", accessTokenResponse.AccessToken);
        }

        private static string RetrieveCachedToken()
        {
            //In a real-world application, we should retrieve the token from a cache service.
            return Environment.GetEnvironmentVariable("token");
        }

        public async Task CreateGroupMemberships(ILogger logger)
        {
            using HttpClient client = httpClientFactory.CreateClient("ZD");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"sankalp.godugu@nationsbenefits.com/token:2T6Xd5Vpw6dwmKvUo8XZj1nPK4o5td9qheoSaER3")));

            foreach (long groupId in Constants.Groups.Select(g => g.Key))
            {
                bool isFirstIter = true;

                string filePath = Constants.MeaTrainingCohort415;
                List<User> usersFromFile = MapFileDataToListOfUsers(filePath);
                if (isFirstIter)
                {
                    isFirstIter = false;
                }
                string query = ConstructFullQuery(usersFromFile);

                List<User> usersFromZendesk = await GetUsers(query, logger);
                if (usersFromZendesk?.Count() > 0)
                {
                    GroupMembershipList groupMemberships = ConstructBulkGroupMembershipAssignmentJSON(usersFromZendesk, groupId);
                    if (groupMemberships.GroupMemberships.Count > 0)
                    {
                        _ = await BulkCreateMemberships(groupMemberships);
                    }
                }
            }
        }

        public async Task DeleteTickets(ILogger logger)
        {
            string searchQuery = "search/count?query=type:ticket group:18731646602263 requester:MEPtoZendesk@nationsbenefits.com created<=2023-12-14 -status:solved";
            PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
            HttpClient client = httpClientFactory.CreateClient("ZD");
            HttpResponseMessage response = await client.GetAsync(searchQuery);
            string result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
            }
            searchQuery = "search?query=type:ticket group:18731646602263 requester:meptozendesk@nationsbenefits.com created<=2023-12-14 -status:solved";

            while (await timer.WaitForNextTickAsync())
            {
                try
                {
                    response = await client.GetAsync(searchQuery);

                    result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Success");
                    }

                    // CONVERT RESPONSE TO C# OBJECT
                    ListSearchResultsResponse lsrr = JsonConvert.DeserializeObject<ListSearchResultsResponse>(result);

                    if (lsrr == null || lsrr.Results == null || lsrr.Results.Count <= 0)
                    {
                        continue;
                    }

                    string ids = "";

                    // CONSTRUCT INPUT FROM API RESPONSE
                    foreach (int ticketId in lsrr.Results.Select(t => t.Id))
                    {
                        ids += ticketId + ",";
                    }
                    ids = ids.Trim(',');

                    // CALL UPDATE MANY TICKETS ENDPOINT to reassign tickets to proper group
                    // 100 at a time, so loop 10 times
                    string destroyQuery = new("tickets/destroy_many?ids=" + ids);

                    response = await client.DeleteAsync(destroyQuery);
                    _ = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Success");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task MoveTickets(ILogger logger)
        {
            PeriodicTimer timer = new(TimeSpan.FromSeconds(10));
            int i = 0;
            while (await timer.WaitForNextTickAsync())
            {
                // HTTP
                using HttpClient client = httpClientFactory.CreateClient("ZD");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"")));


                // API URL
                string baseUrl = "https://nationsbenefits.zendesk.com/api/v2/search/export";
                string query = "?filter[type]=ticket&page[size]=1000&query=group:18731640267543 subject:\" - Request Type: \" requester:meptozendesk@nationsbenefits.com description:\"Order ID: \"";
                if (i % 2 == 1)
                {
                    query = "?filter[type]=ticket&page[size]=1000&query=group:18731640267543 subject:\" - Request Type: \" requester:meptozendesk@nationsbenefits.com description:\"Order ID: \" description:\"Request Type: \"";
                }

                string apiUrl = baseUrl + query;

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                string result = await response.Content.ReadAsStringAsync();
                //File.WriteAllText(@"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Zendesk Integration\Zendesk\exportSearchResultsResponse.json", result);

                // CONVERT RESPONSE TO C# OBJECT
                ExportSearchResultsResponse esrr = JsonConvert.DeserializeObject<ExportSearchResultsResponse>(result);

                UpdateManyTicketsRequest updateManyTicketsRequest = new();
                // CONSTRUCT INPUT FROM API RESPONSE
                foreach (User res in esrr.Users)
                {
                    updateManyTicketsRequest.tickets.Add(new Ticket
                    {
                        Id = res.Id,
                        GroupId = 18731646602263
                    });
                }

                // CALL UPDATE MANY TICKETS ENDPOINT to reassign tickets to proper group
                // 100 at a time, so loop 10 times
                for (int start = 0; start < 1000; start += 100)
                {
                    UpdateManyTicketsRequest subset = new()
                    {
                        tickets = updateManyTicketsRequest.tickets.Skip(start).Take(100).ToList()
                    };
                    string json = JsonConvert.SerializeObject(subset);
                    StringContent sc = new(json, Encoding.UTF8, "application/json");

                    response = await client.PutAsync("https://nationsbenefits.zendesk.com/api/v2/tickets/update_many.json", sc);
                    string temp = await response.Content.ReadAsStringAsync();
                }

                //File.WriteAllText(@"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Zendesk Integration\Zendesk\updateManyTicketsResponse.json", result);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Success");
                }
                else
                {
                    Console.WriteLine("Error");
                }
                i++;
            }
        }

        public async Task<List<Ticket>> GetTicketsWithIncorrectAddress(ILogger logger)
        {
            return await GetTickets($"type:{Constants.Ticket} description:wrong address description:incorrect address");
        }

        #endregion

        #region Private Methods

        private async Task<List<Ticket>> GetTickets(string query)
        {
            try
            {
                HttpClient client = httpClientFactory.CreateClient("ZD");

                int? countTickets = await GetCountAsync($"search/count?query={query}", client);
                if (!countTickets.HasValue || countTickets.Value < 1)
                {
                    return [];
                }

                List<Ticket> tickets = [];
                HttpResponseMessage response = await client.GetAsync($"search/export?filter[type]={Constants.Ticket}&page[size]={Constants.PageSize}&query={query}");
                string result = await response.Content.ReadAsStringAsync();
                ExportSearchResultsResponse? exportSearchResultsResponse = JsonConvert.DeserializeObject<ExportSearchResultsResponse>(result);

                while (exportSearchResultsResponse is not null
                          && exportSearchResultsResponse.Links is not null
                          && exportSearchResultsResponse.Links.Next is not null)
                {
                    response = await client.GetAsync(exportSearchResultsResponse.Links.Next);
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        exportSearchResultsResponse = JsonConvert.DeserializeObject<ExportSearchResultsResponse>(result);
                        if (exportSearchResultsResponse is null || exportSearchResultsResponse.Tickets.Count < 1)
                        {
                            continue;
                        }

                        lock (exportSearchResultsResponse)
                        {
                            tickets.AddRange(exportSearchResultsResponse.Tickets);
                        }
                        response = await client.GetAsync(exportSearchResultsResponse.Links.Next);
                    }
                }

                return tickets;
            }

            catch (JsonException ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
            return [];
        }
        private static async Task<int?> GetCountAsync(string query, HttpClient client)
        {
            HttpResponseMessage response = await client.GetAsync(query);
            string result = await response.Content.ReadAsStringAsync();
            CountResponse? countResponse = JsonConvert.DeserializeObject<CountResponse>(result);
            return countResponse?.Count;
        }
        private static List<User> MapFileDataToListOfUsers(string filePath, int sheetPos = 1)
        {
            List<User> users = [];

            using (XLWorkbook workbook = new(filePath))
            {
                IXLWorksheet worksheet = workbook.Worksheet(sheetPos); // Assuming the data is in the first worksheet

                // Find the header row
                List<string> columns = worksheet.Row(1).CellsUsed().Select(c => c.Value.ToString()).ToList();

                // Find the index of the "Email" column
                string? emailColName = columns.FirstOrDefault(h => h.Contains("email", StringComparison.OrdinalIgnoreCase)) ?? throw new Exception("Email column not found in the Excel file.");
                int emailColumnIndex = columns.IndexOf(emailColName);

                // Read data starting from the row after the header
                List<IXLRow> rows = worksheet.RowsUsed().Skip(1).ToList();
                foreach (IXLRow? row in rows)
                {
                    string email = row.Cell(emailColumnIndex + 1).Value.ToString(); // Excel columns are 1-based
                    users.Add(new User { Email = email });
                }
            }

            return users;
        }
        private static string ConstructFullQuery(List<User> users)
        {
            string query = string.Empty;
            foreach (User user in users)
            {
                query += "user:" + user.Email + " ";
            }
            return query.Trim();
        }
        private static GroupMembershipList ConstructBulkGroupMembershipAssignmentJSON(List<User> users, long groupId)
        {
            GroupMembershipList groupMembershipList = new();
            foreach (User user in users)
            {
                if (user.RoleType is not null)
                {
                    groupMembershipList.GroupMemberships.Add(new GroupMembership
                    {
                        UserId = user.Id,
                        GroupId = groupId
                    });
                }
            }
            return groupMembershipList;
        }
        private async Task<string> BulkCreateMemberships(GroupMembershipList groupMembershipList)
        {
            string json = JsonConvert.SerializeObject(groupMembershipList);
            StringContent sc = new(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClientFactory.CreateClient("ZD").PostAsync("https://nationsbenefits.zendesk.com/api/v2/group_memberships/create_many", sc);
            _ = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode ? "success" : "error";
        }
        public async Task<List<User>> GetUsers(string query, ILogger logger)
        {
            try
            {
                HttpClient client = httpClientFactory.CreateClient("ZD");

                int? countTickets = await GetCountAsync($"search/count?query={query}", client);
                if (!countTickets.HasValue || countTickets.Value < 1)
                {
                    return [];
                }

                List<User> users = [];
                HttpResponseMessage response = await client.GetAsync($"search/export?filter[type]={Constants.User}&page[size]={Constants.PageSize}&query={query}");
                ExportSearchResultsResponse? exportSearchResultsResponse = new();
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    exportSearchResultsResponse = JsonConvert.DeserializeObject<ExportSearchResultsResponse>(result);
                    if (exportSearchResultsResponse is null || exportSearchResultsResponse.Users.Count < 1)
                    {
                        return [];
                    }

                    lock (exportSearchResultsResponse)
                    {
                        users.AddRange(exportSearchResultsResponse.Users);
                    }
                }

                while (exportSearchResultsResponse is not null && exportSearchResultsResponse.Meta is not null && exportSearchResultsResponse.Meta.HasMore)
                {
                    response = await client.GetAsync(exportSearchResultsResponse.Links.Next);
                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        exportSearchResultsResponse = JsonConvert.DeserializeObject<ExportSearchResultsResponse>(result);
                        if (exportSearchResultsResponse is null || exportSearchResultsResponse.Users.Count < 1)
                        {
                            continue;
                        }

                        lock (exportSearchResultsResponse)
                        {
                            users.AddRange(exportSearchResultsResponse.Users);
                        }
                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
            return [];
        }
        public async Task<List<User>> GetInactiveUsers(Filter filter, ILogger logger)
        {
            List<User> users = await GetUsers($"type:{Constants.User}", logger);

            return users.Where(u =>
                   u.Role == Constants.EndUser
                && u.OrganizationId is not null
                && u.OrganizationId != 16807567180439
                && u.LastLoginAt is not null
                && u.LastLoginAtDt < DateTime.Now.AddMonths(-1)
            ).ToList();
        }
        public async Task SendEmailMultiple(List<User> users, ILogger log)
        {
            string smtpServer = Environment.GetEnvironmentVariable("smtpServer");
            int smtpPort = int.Parse(Environment.GetEnvironmentVariable("smtpPort"));
            string smtpUsername = Environment.GetEnvironmentVariable("smtpUsername");
            string smtpPassword = Environment.GetEnvironmentVariable("smtpPassword");
            string fromAddress = Environment.GetEnvironmentVariable("fromAddress");
            string subject = Environment.GetEnvironmentVariable("subject");
            string loginBy = DateTime.Now.AddDays(7).Date.ToLongDateString();

            using SmtpClient client = new(smtpServer, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            int i = 0;
            foreach (var user in users)
            {
                string toAddress = user.Email;
                string body = GetBody(user.Name, loginBy);

                MailMessage message = new(fromAddress, toAddress, subject, body)
                {
                    IsBodyHtml = false
                };

                try
                {
                    if (toAddress == "sankalp.godugu@nationsbenefits.com")
                    {
                        await client.SendMailAsync(message);
                        log.LogInformation("Email sent successfully.");
                    }
                }
                catch (Exception ex)
                {
                    log.LogInformation($"Failed to send email: {ex.Message}");
                }
                i++;
            }
        }
        public async Task SendEmail(List<User> users, ILogger log)
        {
            string smtpServer = Environment.GetEnvironmentVariable("smtpServer");
            int smtpPort = int.Parse(Environment.GetEnvironmentVariable("smtpPort"));
            string smtpUsername = Environment.GetEnvironmentVariable("smtpUsername");
            string smtpPassword = Environment.GetEnvironmentVariable("smtpPassword");
            string fromAddress = Environment.GetEnvironmentVariable("fromAddress");
            string toAddress = Environment.GetEnvironmentVariable("toAddress");
            string subject = Environment.GetEnvironmentVariable("subject");

            using SmtpClient client = new(smtpServer, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            string body = GetBodyClientServices();

            MailMessage message = new(fromAddress, toAddress, subject, body)
            {
                IsBodyHtml = false
            };

            CreateWorkbook(["Organization", "End User", "Email"], Constants.ListOfEndUsersCurr);
            await PopulateWorkbook(users, Constants.ListOfEndUsersCurr);

            Attachment attachment = new(Constants.ListOfEndUsersCurr);
            message.Attachments.Add(attachment);

            try
            {
                if (toAddress == "sankalp.godugu@nationsbenefits.com")
                {
                    await client.SendMailAsync(message);
                    log.LogInformation("Email sent successfully.");
                }
            }
            catch (Exception ex)
            {
                log.LogInformation($"Failed to send email: {ex.Message}");
            }
        }

        public async Task<UpdateManyTicketsResponse> SuspendUsers(bool shouldSuspend, List<User> users, ILogger log)
        {
            try
            {
                List<User> usersFromPreviousReport = ConvertWorkbookToList(Constants.ListOfEndUsersPrev);

                int numUsersSuspended = -1;
                UpdateManyTicketsRequest_Suspended user = new()
                {
                    UserCustom = new()
                    {
                        Suspended = shouldSuspend
                    }
                };
                string json = JsonConvert.SerializeObject(user);
                StringContent sc = new(json, Encoding.UTF8, "application/json");
                var userIds = string.Join(',', users.Select(u => u.Id));
                var client = httpClientFactory.CreateClient("ZD");

                HttpResponseMessage response = await client.PutAsync($"users/update_many?ids={userIds}", sc);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobStatus = JsonConvert.DeserializeObject<UpdateManyTicketsResponse>(result);
                    response = await client.GetAsync($"job_statuses/{jobStatus.JobStatus.Id}");
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
            }

            return new UpdateManyTicketsResponse();
        }

        private static string GetBody(string name, string loginBy) => @$"
Dear {name},

We have noticed that there has been no activity on your NationsBenefits Zendesk Help Center account for some time. In order to ensure the security and integrity of our platform, we kindly ask that you log into your account within the next 7 days. If no login activity is recorded by {loginBy}, your account will be suspended for security purposes.

Please take a moment to log in by clicking on the link below:

https://membersupport.nationsbenefits.com/

Thank you for your attention on this matter.

Regards,

NationsBenefits Zendesk Team

Note: This email and any attachments may contain information that is confidential and/or privileged and prohibited from disclosure or unauthorized use under applicable law. If you are not the intended recipient, you are hereby notified that any disclosure, copying or distribution or taking of action in reliance upon the contents of this transmission is strictly prohibited. If you have received this email in error, you are instructed to notify the sender by reply email and delete it to the fullest extent possible once you have notified the sender of the error.
";

        private static string GetBodyClientServices() => @$"
Dear Client Services,

Please see attached list of all end users suspended in Zendesk due to inactivity/not logging in with 30 days.

Let me know if you have any questions.

Regards,

NationsBenefits Zendesk Team

Note: This email and any attachments may contain information that is confidential and/or privileged and prohibited from disclosure or unauthorized use under applicable law. If you are not the intended recipient, you are hereby notified that any disclosure, copying or distribution or taking of action in reliance upon the contents of this transmission is strictly prohibited. If you have received this email in error, you are instructed to notify the sender by reply email and delete it to the fullest extent possible once you have notified the sender of the error.
";

        private static void SaveWorkbook(List<User> users)
        {
            using var workbook = OpenWorkbook(Constants.ListOfEndUsersCurr);

            var worksheet = workbook.AddWorksheet("Users");

            worksheet.Cell(1, 1).Value = Constants.Name;
            worksheet.Cell(1, 2).Value = Constants.Email;
            worksheet.Cell(1, 3).Value = Constants.LastLoginAt;
            worksheet.Cell(1, 4).Value = Constants.Status;

            int row = 2;
            foreach (var user in users)
            {
                worksheet.Cell(row, 1).Value = user.Name;
                worksheet.Cell(row, 2).Value = user.Email;
                worksheet.Cell(row, 3).Value = user.LastLoginAt;
                worksheet.Cell(row, 4).Value = user.Suspended.Value ? "Suspended" : "Active";
                row++;
            }
        }

        private static XLWorkbook OpenWorkbook(string filePath)
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                _ = Directory.CreateDirectory(directoryPath);
            }

            return File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook();
        }

        private static IXLWorksheet OpenWorksheet(XLWorkbook workbook, string name = "End Users", int pos = 1)
        {
            var sheetExists = workbook.TryGetWorksheet(name, out IXLWorksheet worksheet);
            if (sheetExists)
            {
                return worksheet;
            }
            return workbook.AddWorksheet(name, pos);
        }

        private static void CreateWorkbook(string filePath)
        {
            using var workbook = OpenWorkbook(filePath);
            var worksheet = OpenWorksheet(workbook);
            workbook.SaveAs(filePath);
        }

        private async Task PopulateWorkbook(string[] headers, List<User> users, string filePath)
        {
            Dictionary<long,Task<HttpResponseMessage>> tasks = [];
            var client = httpClientFactory.CreateClient("ZD");

            using var workbook = OpenWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);

            // populate header row
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            // load API calls to get user's organization
            foreach (var user in users)
            {
                tasks.Add(user.Id, client.GetAsync($"organizations/{user.OrganizationId}"));
            }

            // populate data rows
            for (int i = 1; i < users.Count; i++)
            {
                HttpResponseMessage response = await tasks[users[i].Id];
                var result = await response.Content.ReadAsStringAsync();
                var org = JsonConvert.DeserializeObject<ShowOrganizationResponse>(result);

                worksheet.Cell(i + 1, 1).Value = org?.Organization?.Name;
                worksheet.Cell(i + 1, 2).Value = users[i].Name;
                worksheet.Cell(i + 1, 3).Value = users[i].Email;
            }

            workbook.Save();
        }

        private List<User> ConvertWorkbookToList(string filePath)
        {
            var userList = new List<User>();

            // Open the Excel workbook
            using (var workbook = OpenWorkbook(filePath))
            {
                // Assuming the data is in the first worksheet
                var worksheet = workbook.Worksheet(1);

                var headers = worksheet.FirstRow().CellsUsed().Select(c => c.Value.ToString()).ToList();

                // Iterate over rows, starting from the second row (assuming the first row contains headers)
                for (int row = 2; row <= worksheet.LastRowUsed().RowNumber(); row++)
                {
                    var user = new User();

                    foreach (var header in headers)
                    {
                        var cell = worksheet.Cell(row, headers.IndexOf(header) + 1);
                        var property = typeof(User).GetProperty(header);
                        property?.SetValue(user, cell.Value.ToString());
                    }

                    userList.Add(user);
                }
            }

            return userList;
        }

        #endregion
    }
}
