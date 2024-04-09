using Microsoft.Extensions.Logging;
using ZendeskApiIntegration.Model;
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
        public Task CreateGroupMemberships(ILogger logger);

        /// <summary>
        /// Get contact list export Uri from Zendesk asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Zendesk.</returns>
        public Task<IEnumerable<User>> GetInactiveUsers(ILogger logger);

        /// <summary>
        /// Get contact list download Url from Zendesk asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Zendesk.</returns>
        public Task MoveTickets(ILogger logger);

        /// <summary>
        /// Get list of contacts from Zendesk asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Zendesk.</returns>
        public Task DeleteTickets(ILogger logger);

        public Task<IEnumerable<Ticket>> GetTicketsWithIncorrectAddress(ILogger logger);
        Task SendEmail(IEnumerable<User> listOfUsersToSuspend, ILogger log);
    }
}
