using BulkCreateGroupMemberships.Requests;
using BulkCreateGroupMemberships.Responses;
using BulkMoveTickets.Requests;
using Microsoft.Extensions.Logging;

namespace ZendeskContactsProcessJob.ZendeskLayer.Interfaces
{
    /// <summary>
    /// Interface for 
    /// </summary>
    public interface IZendeskClientService
    {
        public Task<UsersResponse> GetUsers(string filePath);

        /// <summary>
        /// Initiate contact list export in Zendesk asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Zendesk.</returns>
        public Task<GroupMembershipList> CreateGroupMemberships(string lang, ILogger logger);

        /// <summary>
        /// Get contact list export Uri from Zendesk asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Zendesk.</returns>
        public Task<Ticket> GetTickets(string lang, ILogger logger);

        /// <summary>
        /// Get contact list download Url from Zendesk asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Zendesk.</returns>
        public Task<Ticket> UpdateTickets(string uri, string lang, ILogger logger);

        /// <summary>
        /// Get list of contacts from Zendesk asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Zendesk.</returns>
        public Task<long> DeleteTickets(string uri, ILogger logger);
    }
}
