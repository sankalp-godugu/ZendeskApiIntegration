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
        public Task GetUserOrganizations(List<User> users, ILogger logger);
        /// <summary>
        /// Initiate contact list export in Zendesk asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Zendesk.</returns>
        public Task CreateGroupMemberships(ILogger logger);

        public Task<List<User>> GetAllEndUsers(string role, ILogger logger);
        public List<User> GetEndUsersNotifiedFromLastReport(List<User> users, ILogger logger);
        /// <summary>
        /// Suspends a list of users
        /// </summary>
        /// <param name="logger"></param>
        /// <returns>count of users suspended</returns>
        public Task<ShowJobStatusResponse> SuspendEndUsers(bool shouldSuspend, List<User> users, ILogger logger);
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
        public Task DeleteTicketsBySearchQuery(ILogger logger);
        public Task DeleteTicketsById(ILogger logger);
        public Task<List<Ticket>> GetTicketsWithIncorrectAddress(ILogger logger);
        public Task<int> NotifyEndUsers(List<User> users, ILogger log);
        public Task<int> NotifyClientServices(ILogger log);
        public Task BuildReport(List<User> users, string status, ILogger log);
    }
}
