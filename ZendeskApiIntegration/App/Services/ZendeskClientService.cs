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
                IEnumerable<User> usersFromFile = MapFileDataToListOfUsers(filePath);
                if (isFirstIter)
                {
                    isFirstIter = false;
                }
                string query = ConstructFullQuery(usersFromFile);

                IEnumerable<User> usersFromZendesk = await GetUsers(query);
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

        public async Task<IEnumerable<User>> GetInactiveUsers(ILogger logger)
        {
            string date = DateTime.Now.AddMonths(-1).Date.ToString("yyyy-MM-dd");
            IEnumerable<User> users = await GetUsers($"type:{Constants.User} role:end-user&last_login_at<{date}");
            List<User> usersNeverLoggedIn = [];
            List<User> usersNotLoggedInPastMonth = [];
            List<User> activeUsers = [];

            foreach (User user in users)
            {
                bool isDate = DateTime.TryParse(user.LastLoginAt, out DateTime lastLoginAt);

                if (!isDate)
                {
                    usersNeverLoggedIn.Add(user);
                }
                else if (lastLoginAt < DateTime.Now.AddMonths(-1))
                {
                    usersNotLoggedInPastMonth.Add(user);
                }
                else
                {
                    activeUsers.Add(user);
                }
            }

            return usersNeverLoggedIn.Concat(usersNotLoggedInPastMonth);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsWithIncorrectAddress(ILogger logger)
        {
            return await GetTickets($"type:{Constants.Ticket} description:wrong address description:incorrect address");
        }

        #endregion

        #region Private Methods

        private static string ConstructApiUrl()
        {
            return "users/search?query=type:user";
        }

        private async Task<IEnumerable<Ticket>> GetTickets(string query)
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

        private async Task<IEnumerable<User>> GetUsers(string query)
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
            catch (JsonException ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
            return null;
        }

        private static async Task<int?> GetCountAsync(string query, HttpClient client)
        {
            HttpResponseMessage response = await client.GetAsync(query);
            string result = await response.Content.ReadAsStringAsync();
            CountResponse? countResponse = JsonConvert.DeserializeObject<CountResponse>(result);
            return countResponse?.Count;
        }

        private static IEnumerable<User> MapFileDataToListOfUsers(string filePath, int sheetPos = 1)
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
                IEnumerable<IXLRow> rows = worksheet.RowsUsed().Skip(1);
                foreach (IXLRow? row in rows)
                {
                    string email = row.Cell(emailColumnIndex + 1).Value.ToString(); // Excel columns are 1-based
                    users.Add(new User { Email = email });
                }
            }

            return users;
        }

        private static string ConstructFullQuery(IEnumerable<User> users)
        {
            string query = string.Empty;
            foreach (User user in users)
            {
                query += "user:" + user.Email + " ";
            }
            return query.Trim();
        }

        private static GroupMembershipList ConstructBulkGroupMembershipAssignmentJSON(IEnumerable<User> users, long groupId)
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

        public async Task SendEmail(IEnumerable<User> users, ILogger log)
        {
            // SMTP server settings
            string smtpServer = "smtp.office365.com";
            int smtpPort = 587; // or any other port your SMTP server uses
            string smtpUsername = "your_smtp_username";
            string smtpPassword = "your_smtp_password";

            // Sender and recipient email addresses
            string fromAddress = "sankalp.godugu@nationsbenefits.com";
            string toAddress = "sankalp.godugu@nationsbenefits.com";

            // Email content
            string subject = "THIS IS A TEST - Zutomation User Suspension Automation";
            string body = "THIS IS A TEST - Zutomation User Suspension Automation";

            // Create a new SMTP client
            using SmtpClient client = new(smtpServer, smtpPort);

            // Enable SSL/TLS if required
            client.EnableSsl = true;

            // Set credentials (if authentication is required)
            client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

            // Create the email message
            MailMessage message = new(fromAddress, toAddress, subject, body);

            try
            {
                // Send the email
                await client.SendMailAsync(message);
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }

        #endregion
    }
}
