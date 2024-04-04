using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Reflection;
using ZendeskApiIntegration.DataLayer.Interfaces;

namespace ZendeskApiIntegration.DataLayer.Services
{
    /// <summary>
    /// Data Layer.
    /// </summary>
    public class DataLayer : IDataLayer
    {
        /// <summary>
        /// Executes a query and returns the collection of objects from the SQL database.
        /// </summary>
        /// <typeparam name="T">The type of the object to return.</typeparam>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameters">The dictionary of SQL parameters to pass to the stored procedure.</param>
        /// <param name="connectionString">Connection string</param>
        /// <returns>The collection of objects returned from the executed query.</returns>
        public async Task<List<T>> ExecuteReader<T>(string procedureName, Dictionary<string, object> parameters, string connectionString, ILogger logger)
        {
            logger.LogInformation($"Started calling stored procedure {procedureName} with parameters: {string.Join(", ", parameters.Select(p => $"{p.Key} = {p.Value}"))}");
            List<T> list = new();
            using (SqlConnection sqlConnection = new(connectionString))
            {
                await sqlConnection.OpenAsync();
                try
                {
                    using SqlCommand sqlCommand = new();
                    sqlCommand.CommandTimeout = GetSqlCommandTimeout(logger);
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = procedureName;

                    if (parameters.Count > 0)
                    {
                        sqlCommand.Parameters.AddRange(GetSqlParameters(parameters).ToArray());
                    }
                    SqlDataReader dataReader = await sqlCommand.ExecuteReaderAsync();

                    list = DataReaderMapToList<T>(dataReader, logger);
                }
                catch (Exception ex)
                {
                    logger.LogError($"{procedureName} failed with exception: {ex.Message}");
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
            logger.LogInformation($"Ended calling stored procedure {procedureName}");
            return list;
        }

        /// <summary>
        /// Inserts the data into the table.
        /// </summary>
        /// <typeparam name="T">Generic parameter.</typeparam>
        /// <param name="procedureName">Procedure name.</param>
        /// <param name="ZendeskContactId">Zendesk contact id.</param>
        /// <param name="action">Zendesk API action.</param>
        /// <param name="currentProcessId">Current process id.</param>
        /// <param name="logger">Logger</param>
        /// <param name="connectionString">Connection string.</param>
        /// <returns>Returns the collection of objects.</returns>
        public async Task<int> ExecuteNonQueryForZendesk(string procedureName, long ZendeskContactId, string action, long currentProcessId, string connectionString, ILogger logger)
        {
            using SqlConnection connection = new(connectionString);
            await connection.OpenAsync();

            using SqlCommand command = new(procedureName, connection);
            command.CommandType = CommandType.StoredProcedure;

            // Input parameters
            _ = command.Parameters.AddWithValue("@ZendeskContactId", ZendeskContactId);
            _ = command.Parameters.AddWithValue("@Action", action);
            _ = command.Parameters.AddWithValue("@IsProcessed", currentProcessId);

            // Output parameter
            SqlParameter resultParameter = new("@Result", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            _ = command.Parameters.Add(resultParameter);

            try
            {
                _ = await command.ExecuteNonQueryAsync();

                // Retrieve the result code
                int result = (int)resultParameter.Value;

                // Log the result
                logger.LogInformation($"UpdateZendeskReferenceForContact result: {result}");

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error updating Zendesk contact reference: {ex.Message}");
                return -1;
            }
            finally
            {
                connection.Close();
            }
        }

        #region Private Methods

        /// <summary>
        /// Gets the sql command timeout.
        /// </summary>
        /// <returns>Returns the sql command timeout.</returns>
        private static int GetSqlCommandTimeout(ILogger logger)
        {
            try
            {
                string sqlCommandTimeOut = Environment.GetEnvironmentVariable("SQLCommandTimeOut");
                return !string.IsNullOrEmpty(sqlCommandTimeOut) && int.TryParse(sqlCommandTimeOut, out int parsedValue) ? parsedValue : 300;
            }
            catch (Exception ex)
            {
                logger.LogError($"TimeOut with Exception: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Converts from data reader to a collection of generic objects.
        /// </summary>
        /// <typeparam name="T">The type of the object to return.</typeparam>
        /// <param name="dataReader">The DataReader.</param>
        /// <returns>Returns the parsed object from the data reader.</returns>
        private static List<T> DataReaderMapToList<T>(IDataReader dataReader, ILogger logger)
        {
            List<T> list = new();
            try
            {
                T obj = default!;
                while (dataReader.Read())
                {
                    obj = Activator.CreateInstance<T>();
                    if (obj != null)
                    {
                        foreach (PropertyInfo property in obj.GetType().GetProperties())
                        {
                            if (property != null && !Equals(dataReader[property.Name], DBNull.Value))
                            {
                                property.SetValue(obj, dataReader[property.Name], null);
                            }
                        }
                        list.Add(obj);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occured while parsing the List to table with Exception: {ex.Message}");
                return list;
            }
        }

        /// <summary>
        /// Gets the sql parameters.
        /// </summary>
        /// <param name="parameters">The dictionary of SQL parameters.</param>
        /// <returns>Returns the collection of sql parameters.</returns>
        private List<SqlParameter> GetSqlParameters(Dictionary<string, object> parameters)
        {
            return parameters.Select(sp => new SqlParameter(sp.Key, sp.Value)).ToList();
        }

        #endregion
    }
}
