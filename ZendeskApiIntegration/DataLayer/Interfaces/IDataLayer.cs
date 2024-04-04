using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZendeskApiIntegration.DataLayer.Interfaces
{
    // Interface for data layer.
    public interface IDataLayer
    {
        /// <summary>
        /// Executes a stored procedure and returns the result as a byte array.
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="sqlParams">The dictionary of SQL parameters to pass to the stored procedure.</param>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="logger">Logger</param>
        /// <returns>The result of the stored procedure as a byte array.</returns>
        Task<List<T>> ExecuteReader<T>(string procedureName, Dictionary<string, object> parameters, string connectionString, ILogger logger);


        /// <summary>
        /// Inserts the data into the table.
        /// </summary>
        /// <typeparam name="T">Generic parameter.</typeparam>
        /// <param name="procedureName">Procedure name.</param>
        /// <param name="ZendeskContactId">Zendesk contact id.</param>
        /// <param name="action">Zendesk API action.</param>
        /// <param name="currentProcessId">Current process id.</param>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="logger">Logger</param>
        /// <returns>Returns the collection of objects.</returns>
        Task<int> ExecuteNonQueryForZendesk(string procedureName, long ZendeskContactId, string action, long currentProcessId, string connectionString, ILogger logger);
    }
}
