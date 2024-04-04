using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ZendeskApiIntegration.App.Interfaces;
using ZendeskApiIntegration.Utilities;

namespace ZendeskApiIntegration.App.Services
{
    /// <summary>
    /// Zendesk client service.
    /// </summary>
    public class ZendeskClientService(IHttpClientFactory httpClientFactory, IConfiguration config) : IZendeskClientService
    {
        #region Public Methods

        /// <summary>
        /// Initiate the export for Zendesk contact list async
        /// </summary>


        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the API request body for Zendesk.
        /// </summary>
        /// <param name="contactsToDeleteFromZendesk">Contacts to Process.<see cref="ContactsToDelete"/></param>
        /// <returns>Returns the string content.</returns>
        private string GetDeleteRequestQueryForZendesk(IEnumerable<string> contactsToDeleteFromZendesk, ILogger logger)
        {
            try
            {
                string contactIds = "";
                foreach (string contact in contactsToDeleteFromZendesk)
                {
                    contactIds += $"{contact},";
                }
                return contactIds.TrimEnd(',');
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed in processing the query string arguments for Zendesk with exception message: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the Zendesk http client.
        /// </summary>
        /// <returns>Returns the http client to make API requests.</returns>
        private HttpClient GetZendeskHttpClient()
        {
            _ = new HttpClientHandler() { AllowAutoRedirect = false };
            return httpClientFactory.CreateClient("Zendesk");
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

        private void SetCacheToken(AccessTokenResponse accessTokenResponse)
        {
            //In a real-world application we should store the token in a cache service and set an TTL.
            Environment.SetEnvironmentVariable("token", accessTokenResponse.AccessToken);
        }

        private string RetrieveCachedToken()
        {
            //In a real-world application, we should retrieve the token from a cache service.
            return Environment.GetEnvironmentVariable("token");
        }

        public async Task CreateGroupMemberships(string lang, ILogger logger)
        {
            BulkCreateGroupMemberships bcgm = new(httpClientFactory);
            await bcgm.Initialize();
        }

        public async Task MoveTickets(string uri, string lang, ILogger logger)
        {
            BulkMoveTickets bmt = new(httpClientFactory);
            await bmt.Initialize();
        }

        public async Task DeleteTickets(string uri, ILogger logger)
        {
            BulkDeleteTickets bdt = new(httpClientFactory);
            await bdt.Initialize();
        }

        async Task IZendeskClientService.SuspendUsers(string lang, ILogger logger)
        {
            BulkSuspendUsers bsu = new(httpClientFactory);
            await bsu.Initialize();
        }

        #endregion
    }
}
