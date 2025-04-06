using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ArtAttack.Shared
{
    public interface IDatabaseProvider
    {
        IDbConnection CreateConnection(string connectionString);
    }

    public class SqlDatabaseProvider : IDatabaseProvider
    {
        /// <summary>
        /// Creates a new SQL Server connection with the specified connection string.
        /// </summary>
        /// <param name="connectionString">A valid SQL Server connection string. Cannot be null or empty.</param>
        /// <returns>A new SqlConnection instance that can be used to connect to a SQL Server database.</returns>
        /// <remarks>
        /// The returned connection is not opened by this method.
        /// The caller is responsible for opening, using, and disposing of the connection.
        /// </remarks>
        public IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}