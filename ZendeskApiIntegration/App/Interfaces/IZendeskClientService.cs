using Microsoft.Extensions.Logging;
using ZendeskApiIntegration.Model.Requests;
using ZendeskApiIntegration.Model.Responses;

namespace ZendeskApiIntegration.App.Interfaces
{
    /// <summary>
    /// Interface for 
    /// </summary>
    public interface IZendeskClientService
    {
        /// <summary>
        /// Initiate contact list export in Zendesk asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Zendesk.</returns>
        public Task CreateGroupMemberships(string lang, ILogger logger);

        /// <summary>
        /// Get contact list export Uri from Zendesk asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Zendesk.</returns>
        public Task SuspendUsers(string lang, ILogger logger);

        /// <summary>
        /// Get contact list download Url from Zendesk asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Zendesk.</returns>
        public Task MoveTickets(string uri, string lang, ILogger logger);

        /// <summary>
        /// Get list of contacts from Zendesk asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Zendesk.</returns>
        public Task DeleteTickets(string uri, ILogger logger);
    }
}
