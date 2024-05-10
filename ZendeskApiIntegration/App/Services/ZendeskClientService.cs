﻿using ClosedXML.Excel;
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
using static ZendeskApiIntegration.Utilities.Constants;
using Columns = ZendeskApiIntegration.Utilities.Constants.Columns;
using Filter = ZendeskApiIntegration.Model.Filter;
using Sheets = ZendeskApiIntegration.Utilities.Constants.Sheets;
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
                string query = $"?filter[type]=ticket&page[size]={Limits.PageSize}&query=group:18731640267543 subject:\" - Request Type: \" requester:meptozendesk@nationsbenefits.com description:\"Order ID: \"";
                if (i % 2 == 1)
                {
                    query = $"?filter[type]=ticket&page[size]={Limits.PageSize}&query=group:18731640267543 subject:\" - Request Type: \" requester:meptozendesk@nationsbenefits.com description:\"Order ID: \" description:\"Request Type: \"";
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
                for (int start = 0; start < Limits.PageSize; start += Limits.BatchSize)
                {
                    UpdateManyTicketsRequest subset = new()
                    {
                        tickets = updateManyTicketsRequest.tickets.Skip(start).Take(Limits.BatchSize).ToList()
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
            return await GetTickets($"type:{Types.Ticket} description:wrong address description:incorrect address");
        }
        public async Task CreateGroupMemberships(ILogger log)
        {
            string filePath = CohortFilePath;
            List<User> usersFromFile = GetUsersFromFile(filePath);
            string query = ConstructFullQuery(usersFromFile);
            List<User> usersFromZendesk = await GetUsers(query, log);
            if (usersFromZendesk?.Count > 0)
            {
                foreach (long groupId in Constants.Groups.Select(g => g.Key))
                {
                    GroupMembershipsWrapper groupMembershipsWrapper = ConstructBulkGroupMembershipAssignmentJSON(usersFromZendesk, groupId);
                    if (groupMembershipsWrapper.GroupMemberships.Count > 0)
                    {
                        int totalGroupMemberships = groupMembershipsWrapper.GroupMemberships.Count;
                        int batchSize = Limits.BatchSize;
                        int remainingGroupMemberships = totalGroupMemberships;
                        bool hasMoreToProcess = true;

                        for (int i = 0; hasMoreToProcess; i++)
                        {
                            GroupMembershipsWrapper subset = new()
                            {
                                GroupMemberships = groupMembershipsWrapper.GroupMemberships.Skip(batchSize * i).Take(batchSize).ToList()
                            };
                            ShowJobStatusResponse jobStatusResponse = await BulkCreateMemberships(subset, log);

                            remainingGroupMemberships = totalGroupMemberships - (batchSize * (i + 1));
                            hasMoreToProcess = remainingGroupMemberships > 0;
                        }
                    }
                }
            }
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
                HttpResponseMessage response = await client.GetAsync($"search/export?filter[type]={Types.Ticket}&page[size]={Limits.PageSize}&query={query}");
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
        private static List<User> GetUsersFromFile(string filePath, int sheetPos = 1)
        {
            using XLWorkbook workbook = OpenWorkbook(filePath);
            IXLWorksheet worksheet = OpenWorksheet(workbook, Columns.Cohort, sheetPos); // Assuming the data is in the first worksheet

            // Find the header row
            List<string> columns = worksheet.Row(1).CellsUsed().Select(c => c.Value.ToString()).ToList();

            // Find the index of the "Email" column
            string? emailCol = columns.FirstOrDefault(h => h.Contains("email", StringComparison.OrdinalIgnoreCase)) ?? throw new Exception("Email column not found in the Excel file.");
            int emailColumnIndex = columns.IndexOf(emailCol);

            // Read data starting from the row after the header
            List<IXLRow> rows = worksheet.RowsUsed().Skip(1).ToList();
            List<User> users = [];
            foreach (IXLRow? row in rows)
            {
                string email = row.Cell(emailColumnIndex + 1).Value.ToString(); // Excel columns are 1-based
                users.Add(new User { Email = email });
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
        private static GroupMembershipsWrapper ConstructBulkGroupMembershipAssignmentJSON(List<User> users, long groupId)
        {
            GroupMembershipsWrapper groupMembershipList = new();
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
        private async Task<ShowJobStatusResponse> BulkCreateMemberships(GroupMembershipsWrapper groupMembershipsWrapper, ILogger log)
        {
            HttpClient client = httpClientFactory.CreateClient("ZD");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            string json = JsonConvert.SerializeObject(groupMembershipsWrapper);
            StringContent sc = new(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("group_memberships/create_many", sc);
            ShowJobStatusResponse showJobStatusResponse = new();
            if (response.IsSuccessStatusCode)
            {
                log.LogInformation($"Queued job to bulk create memberships...");

                string result = await response.Content.ReadAsStringAsync();
                showJobStatusResponse = JsonConvert.DeserializeObject<ShowJobStatusResponse>(result);
                do
                {
                    Thread.Sleep(Limits.SleepTime);
                    response = await client.GetAsync($"job_statuses/{showJobStatusResponse.JobStatus.Id}");
                    if (response.IsSuccessStatusCode)
                    {
                        //log.LogInformation($"End users suspended successfully...");
                        result = await response.Content.ReadAsStringAsync();
                        showJobStatusResponse = JsonConvert.DeserializeObject<ShowJobStatusResponse>(result);
                    }
                } while (showJobStatusResponse?.JobStatus.Status is JobStatuses.Queued
                        or JobStatuses.Working
                        );
            }
            return showJobStatusResponse;
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
                HttpResponseMessage response = await client.GetAsync($"search/export?filter[type]={Types.User}&page[size]={Limits.PageSize}&query={query}");
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
            List<User> endUsers = await GetUsers($"type:{filter.Type} role:{filter.Role} -organization_id:{filter.OrgId} -organization_id:none", logger);

            return endUsers.Where(u =>
                   u.Role == Roles.EndUser
                   && u.OrganizationId is not null
                   && u.OrganizationId != Organizations.Nations
                && u.LastLoginAt is not null
                && u.LastLoginAtDt < DateTime.Now.AddMonths(-1)
            ).ToList();
        }
        public List<User> GetEndUsersFromLastReport(List<User> inactiveUsers, ILogger logger)
        {
            return ConvertWorkbookToList(FilePath.ListOfEndUsersNotifiedLastWeek);
        }

        public async Task GetUserOrganizations(List<User> users, ILogger logger)
        {
            HttpClient client = httpClientFactory.CreateClient("ZD");
            List<Task<HttpResponseMessage>> tasks = new();
            
            foreach(var user in users)
            {
                HttpResponseMessage response = await client.GetAsync($"organizations/{user.OrganizationId}");
                string result = await response.Content.ReadAsStringAsync();
                ShowOrganizationResponse? org = JsonConvert.DeserializeObject<ShowOrganizationResponse>(result);
                user.OrganizationName = org?.Organization?.Name;
            }
        }

        public async Task<ShowJobStatusResponse> SuspendUsers(bool shouldSuspend, List<User> users, ILogger log)
        {
            ShowJobStatusResponse showJobStatusResponse = new();

            try
            {
                UpdateManyTicketsRequest_Suspended user = new()
                {
                    UserCustom = new()
                    {
                        Suspended = shouldSuspend
                    }
                };

                string json = JsonConvert.SerializeObject(user);
                StringContent sc = new(json, Encoding.UTF8, "application/json");
                
                HttpClient client = httpClientFactory.CreateClient("ZD");

                log.LogInformation($"Suspending end users...");
                int numAttempts = 0;
                for (int i = 0; i < i + users.Count; i += Limits.BatchSize) {

                    int startIndex = i;
                    int endIndex = Math.Min(i + Limits.BatchSize, users.Count);
                    List<User> batchOfUsers = users.GetRange(startIndex, endIndex - startIndex);
                    string userIds = string.Join(',', batchOfUsers.Select(u => u.Id));
                    numAttempts = 0;
                    while (numAttempts < Limits.MaxAttempts)
                    {
                        HttpResponseMessage response = await client.PutAsync($"users/update_many?ids={userIds}", sc);
                        if (response.IsSuccessStatusCode)
                        {
                            log.LogInformation($"Queued job to suspend end users...");
                            string result = await response.Content.ReadAsStringAsync();
                            showJobStatusResponse = JsonConvert.DeserializeObject<ShowJobStatusResponse>(result);
                            do
                            {
                                response = await client.GetAsync($"{showJobStatusResponse.JobStatus.Url}");
                                if (response.IsSuccessStatusCode)
                                {
                                    result = await response.Content.ReadAsStringAsync();
                                    showJobStatusResponse = JsonConvert.DeserializeObject<ShowJobStatusResponse>(result);
                                }
                            } while (showJobStatusResponse.JobStatus.Status == JobStatuses.Queued
                                        || showJobStatusResponse.JobStatus.Status is JobStatuses.Working);
                        }
                        numAttempts++;
                    }
                }

                await BuildReport(showJobStatusResponse, users, log);
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
            }

            return showJobStatusResponse;
        }
        private async Task BuildReport(ShowJobStatusResponse showJobStatusResponse, List<User> users, ILogger log)
        {
            try
            {
                XLWorkbook workbook = CreateWorkbook(Columns.HeadersSuspendedUsers, users, FilePath.ListOfEndUsersSuspended, [Sheets.EndUsers]);
                _ = workbook.TryGetWorksheet(Sheets.EndUsers, out IXLWorksheet sheet);
                ApplyFiltersAndSaveReport(workbook);

                foreach (JobResult response in showJobStatusResponse.JobStatus.Results)
                {
                    User userToAdd = users.FirstOrDefault(user => response.Id == user.Id);
                    await AddUserToSheet(userToAdd, response.SuspensionStatus, GetCurrentTimeInEst(), sheet, workbook, Columns.HeadersSuspendedUsers);
                }
            }
            catch (Exception ex)
            {
                log.LogInformation("Error building report: " + ex.Message);
            }

        }
        /// <summary>
        /// sends an email to client services with list of end users that have been suspended
        /// </summary>
        /// <param name="users">list of inactive non-Nations end users that are part of an organization</param>
        /// <param name="log"></param>
        /// <returns>0 if success, -1 if fail</returns>
        public async Task<int> NotifyClientServices(List<User> users, ILogger log)
        {
            string smtpServer = Environment.GetEnvironmentVariable("smtpServer");
            int smtpPort = int.Parse(Environment.GetEnvironmentVariable("smtpPort"));
            string smtpUsername = Environment.GetEnvironmentVariable("smtpUsername");
            string smtpPassword = Environment.GetEnvironmentVariable("smtpPassword");
            string fromAddress = Emails.FromAddress;
            string toAddress = Emails.ToAddress;
            string subject = Emails.SubjectClientServices;

            using SmtpClient client = new(smtpServer, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                Timeout = Emails.Timeout
            };

            string body = GetBodyClientServices();

            MailMessage message = new(fromAddress, toAddress, subject, body)
            {
                IsBodyHtml = false
            };
            message.CC.Add(Emails.ToAddressCC);
            message.CC.Add(Emails.EmailNationsDavidDandridge);

            Attachment attachment = new(FilePath.ListOfEndUsersSuspended);
            message.Attachments.Add(attachment);

            int numAttempts = 0;
            while (numAttempts < Limits.MaxAttempts)
            {
                try
                {
                    log.LogInformation($"Sending email to Client Services...");
                    client.Send(message);
                    log.LogInformation("Email sent successfully.");
                    break;
                }
                catch (SmtpException)
                {
                    log.LogInformation($"Sending email timed out.");
                }
                catch (Exception ex)
                {
                    log.LogInformation($"Failed to send email: {ex.Message}");
                }
                numAttempts++;
                Thread.Sleep(Limits.SleepTime);
            }

            return 0;
        }
        /// <summary>
        /// sends an email to each non-Nations end user part of an org that is at risk of suspension due to inactivity
        /// </summary>
        /// <param name="users">list of inactive non-Nations end users that are part of an organization</param>
        /// <param name="log"></param>
        /// <returns>0 if success, -1 if fail</returns>
        /// <summary>
        /// suspends users if appear in weekly report twice
        /// </summary>
        /// <param name="shouldSuspend">flag that determines whether to suspend or unsuspend users</param>
        /// <param name="users">list of inactive end users from this week's report</param>
        /// <param name="log"></param>
        /// <returns></returns>
        public async Task<int> NotifyEndUsers(List<User> users, ILogger log)
        {
            string smtpServer = Environment.GetEnvironmentVariable("smtpServer");
            int smtpPort = int.Parse(Environment.GetEnvironmentVariable("smtpPort"));
            string smtpUsername = Environment.GetEnvironmentVariable("smtpUsername");
            string smtpPassword = Environment.GetEnvironmentVariable("smtpPassword");
            string fromAddress = Emails.FromAddress;
            string subject = Emails.SubjectEndUsers;
            string loginBy = GetDate6DaysLaterInEst();

            using SmtpClient client = new(smtpServer, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                Timeout = Limits.Timeout
            };

            List<User> usersNotifiedSuccessfully = [];
            List<User> usersFailedToNotify = [];
            users = [.. users.OrderBy(u => u.Id)];

            XLWorkbook workbook = CreateWorkbook(Columns.HeadersNotifiedUsers, users, FilePath.ListOfEndUsersNotified, [Sheets.EndUsers]);
            _ = workbook.TryGetWorksheet(Sheets.EndUsers, out IXLWorksheet sheet);
            ApplyFiltersAndSaveReport(workbook);

            foreach (User user in users)
            {
                string toAddress = user.Email;
                string body = GetBodyEndUser(user.Name, loginBy);

                MailMessage message = new(fromAddress, toAddress, subject, body)
                {
                    IsBodyHtml = false
                };

                int numAttempts = 0;
                while (numAttempts < Limits.MaxAttempts)
                {
                    try
                    {
                        log.LogInformation($"Sending email to {user.Name} ({user.Email})...");
                        client.Send(message);
                        usersNotifiedSuccessfully.Add(user);
                        await AddUserToSheet(user, JobStatuses.Completed, GetCurrentTimeInEst(), sheet, workbook, Columns.HeadersNotifiedUsers);
                        break;
                    }
                    catch (Exception ex)
                    {
                        log.LogInformation($"Failed to send email: {ex.Message}");
                        usersFailedToNotify.Add(user);
                        await AddUserToSheet(user, JobStatuses.Failed, GetCurrentTimeInEst(), sheet, workbook, Columns.HeadersNotifiedUsers);
                    }

                    numAttempts++;
                }
                Thread.Sleep(Limits.SleepTime);
            }

            return 0;
        }
        private static string GetBodyEndUser(string name, string loginBy)
        {
            return @$"
Dear {name},

We have noticed that there has been no activity on your NationsBenefits Zendesk Help Center account for some time. To ensure the security and integrity of our platform, we kindly ask that you log into your account within the next 7 days. If no login activity is recorded by 11:59 pm EST on {loginBy}, your account will be suspended for security purposes.

Please take a moment to log in by clicking on the link below:

{MemberSupportEmail}

Thank you for your attention on this matter.

Regards,

NationsBenefits Zendesk Team

Note: This email and any attachments may contain information that is confidential and/or privileged and prohibited from disclosure or unauthorized use under applicable law. If you are not the intended recipient, you are hereby notified that any disclosure, copying or distribution or taking of action in reliance upon the contents of this transmission is strictly prohibited. If you have received this email in error, you are instructed to notify the sender by reply email and delete it to the fullest extent possible once you have notified the sender of the error.
";
        }
        private static string GetBodyClientServices()
        {
            return @$"
Hi Team,

Please see attached list of all external end users suspended in Zendesk due to inactivity/not logging in within 30 days.

Let me know if you have any questions.

Regards,

NationsBenefits Zendesk Team

Note: This email and any attachments may contain information that is confidential and/or privileged and prohibited from disclosure or unauthorized use under applicable law. If you are not the intended recipient, you are hereby notified that any disclosure, copying or distribution or taking of action in reliance upon the contents of this transmission is strictly prohibited. If you have received this email in error, you are instructed to notify the sender by reply email and delete it to the fullest extent possible once you have notified the sender of the error.
";
        }
        private static void SaveWorkbook(List<User> users)
        {
            using XLWorkbook workbook = OpenWorkbook(FilePath.ListOfEndUsersSuspended);

            IXLWorksheet worksheet = workbook.AddWorksheet("Users");

            worksheet.Cell(1, 1).Value = Columns.Name;
            worksheet.Cell(1, 2).Value = Columns.Email;
            worksheet.Cell(1, 3).Value = Columns.LastLoginAt;
            worksheet.Cell(1, 4).Value = Columns.Status;

            int row = 2;
            foreach (User user in users)
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

            int numberAttempts = 0;
            int maxAttempts = Limits.MaxAttempts;
            while (numberAttempts <= maxAttempts)
            {
                try
                {
                    return File.Exists(filePath) ? new(filePath) : new();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(Limits.SleepTime);
                    numberAttempts++;
                }
            }
            return new();
        }
        private static IXLWorksheet OpenWorksheet(XLWorkbook workbook, string? name = null, int pos = 1)
        {
            bool sheetExists = workbook.TryGetWorksheet(name, out IXLWorksheet worksheet);

            return sheetExists ? worksheet : workbook.AddWorksheet(name, pos);
        }
        private XLWorkbook CreateWorkbook(string[] headers, List<User> users, string filePath, string[] sheets)
        {
            XLWorkbook workbook = OpenWorkbook(filePath);

            foreach (string sheet in sheets)
            {
                IXLWorksheet worksheet = OpenWorksheet(workbook, sheet);

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                }
            }

            workbook.SaveAs(filePath);
            return workbook;
        }
        public static void AddHeaders(string[] headers, IXLWorksheet worksheet)
        {
            // populate header row
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }
        }
        private async Task AddUserToSheet(User user, string status, DateTime time, IXLWorksheet worksheet, XLWorkbook workbook, string[] headers)
        {
            try
            {
                if (worksheet.FirstRow().IsEmpty())
                {
                    AddHeaders(headers, worksheet);
                }

                HttpClient client = httpClientFactory.CreateClient("ZD");
                HttpResponseMessage response = await client.GetAsync($"organizations/{user.OrganizationId}");
                string result = await response.Content.ReadAsStringAsync();
                ShowOrganizationResponse? org = JsonConvert.DeserializeObject<ShowOrganizationResponse>(result);

                int row = worksheet.LastRowUsed() is null ? 2 : worksheet.LastRowUsed().RowNumber() + 1;
                worksheet.Cell(row, 1).Value = org?.Organization?.Name;
                worksheet.Cell(row, 2).Value = user.Name;
                worksheet.Cell(row, 3).Value = user.Email;
                worksheet.Cell(row, 4).Value = status.ToString();
                worksheet.Cell(row, 5).Value = time.ToString();
                workbook.Save();
            }
            catch (Exception)
            {

            }
        }
        private static void ApplyFiltersAndSaveReport(XLWorkbook workbook)
        {
            // Check if the worksheet exists
            //DeleteWorkbook(filePath);

            foreach (IXLWorksheet? worksheet in workbook.Worksheets)
            {

                // Add a new worksheet with the same name
                ClearWorksheet(worksheet);

                // Format header row
                FormatHeaderRow(worksheet.Row(1));

                // Freeze header row
                worksheet.SheetView.FreezeRows(1);

                // Set number formats for specific columns
                SetNumberFormats(worksheet);

                // Set column widths
                SetColumnWidths(worksheet);

                // Apply filters to specific columns
                //ApplyFilters(worksheet, 14, "Reimbursement");
                //ApplyFilters(worksheet, 5, "2024");
            }

            SaveWorkbook(FilePath.ListOfEndUsersNotified, workbook);
        }
        private static void ClearWorksheet(IXLWorksheet worksheet)
        {
            // Get the worksheet or add a new one if it doesn't exist
            if (worksheet == null)
            {
                return;
            }
            else
            {
                // Clear existing data if the worksheet already exists
                _ = worksheet.AutoFilter.Clear();
                _ = worksheet.Clear();
            }
        }
        private static void SaveWorkbook(string filePath, XLWorkbook workbook)
        {
            // Save the workbook
            if (!string.IsNullOrEmpty(filePath))
            {
                // If the workbook has a file path, it means it has been previously saved
                workbook.SaveAs(filePath); // Use Save() to save the workbook using its existing file name or location
            }
            else
            {
                // If the workbook has not been previously saved, use SaveAs()
                workbook.SaveAs(filePath); // Provide the desired file path
            }
        }
        private static void FormatHeaderRow(IXLRow headerRow)
        {
            headerRow.Style.Fill.BackgroundColor = XLColor.Orange;
            headerRow.Style.Font.Bold = true;
        }
        private static void SetNumberFormats(IXLWorksheet worksheet)
        {
            foreach (IXLColumn column in worksheet.Columns())
            {
                XLDataType dataType = GetColumnType(column);
                string format = GetFormatForDataType(dataType);
                column.Style.NumberFormat.Format = format;
            }
        }
        private static XLDataType GetColumnType(IXLColumn column)
        {
            // Check the data type of the cells in the column
            bool isNumeric = column.Cells().All(cell => cell.DataType == XLDataType.Number);
            bool isDate = column.Cells().All(cell => cell.DataType == XLDataType.DateTime);

            return isNumeric ? XLDataType.Number : isDate ? XLDataType.DateTime : XLDataType.Text;
        }
        private static string GetFormatForDataType(XLDataType dataType)
        {
            return dataType switch
            {
                XLDataType.Number => "$#,##0.00",
                XLDataType.DateTime => "MM/dd/yyyy",
                XLDataType.Text => "@",// General text format
                _ => "",
            };
        }
        private static void SetColumnWidths(IXLWorksheet worksheet, int width = 20)
        {
            foreach (IXLColumn column in worksheet.ColumnsUsed())
            {
                column.Width = width;
            }
        }
        private static void ApplyFilters(IXLWorksheet worksheet, int columnIndex, string filterCriteria)
        {

            _ = worksheet.SetAutoFilter().Column(columnIndex).AddFilter(filterCriteria);
        }
        private static List<User> ConvertWorkbookToList(string filePath, string sheetName = Columns.EndUsers)
        {
            List<User> userList = [];

            // Open the Excel workbook
            using XLWorkbook workbook = OpenWorkbook(filePath);
            if (workbook.Worksheets.Count < 1)
            {
                return [];
            }

            // Assuming the data is in the first worksheet
            IXLWorksheet worksheet = OpenWorksheet(workbook, sheetName);
            List<string> headers = worksheet.FirstRow().CellsUsed().Select(c => c.Value.ToString()).ToList();

            // Iterate over rows, starting from the second row (assuming the first row contains headers)
            int lastRowUsed = worksheet.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= lastRowUsed; row++)
            {
                User user = new();

                foreach (string? header in headers)
                {
                    IXLCell cell = worksheet.Cell(row, headers.IndexOf(header) + 1);
                    System.Reflection.PropertyInfo? property = typeof(User).GetProperty(header);
                    property?.SetValue(user, cell.Value.ToString());
                }

                userList.Add(user);
            }

            return userList;
        }

        // Define a custom equality comparer based on the UserId property
        public class UserEmailEqualityComparer : IEqualityComparer<User>
        {
            public bool Equals(User x, User y)
            {
                // Compare emails case-insensitively
                return string.Equals(x.Email, y.Email, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(User obj)
            {
                // Use the hash code of the email
                return obj.Email?.GetHashCode() ?? 0;
            }
        }

        #endregion
    }
}
