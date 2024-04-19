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
        public Task<List<User>> GetInactiveUsers(ILogger logger);

        /// <summary>
        /// Suspends a list of users
        /// </summary>
        /// <param name="logger"></param>
        /// <returns>count of users suspended</returns>
        public Task<UpdateManyTicketsResponse> SuspendUsers(bool shouldSuspend, List<User> users, ILogger logger);

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

        public Task<List<Ticket>> GetTicketsWithIncorrectAddress(ILogger logger);
        public Task SendEmailMultiple(List<User> listOfUsersToSuspend, ILogger log);
        public Task SendEmail(string toAddress, string orgName, string name, ILogger log);
    }
}
