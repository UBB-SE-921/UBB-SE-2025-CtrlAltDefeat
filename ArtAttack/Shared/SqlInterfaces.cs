using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ArtAttack.Shared
{
    /// <summary>
    /// Defines a contract for creating database connections.
    /// </summary>
    public interface IDatabaseProvider
    {
        IDbConnection CreateConnection(string connectionString);
    }

    /// <summary>
    /// Provides an implementation of the IDatabaseProvider for creating SQL Server connections.
    /// </summary>
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

    /// <summary>
    /// Defines a contract for executing database commands asynchronously.
    /// </summary>
    public interface IDatabaseCommand
    {
        Task<IDataReader> ExecuteReaderAsync(IDbCommand command, CancellationToken cancellationToken);
    }


    /// <summary>
    /// Implements the IDatabaseCommand interface for executing SQL database commands.
    /// </summary>
    public class SqlDatabaseCommand : IDatabaseCommand
    {
        private readonly IDbConnection _connection;

        public SqlDatabaseCommand(IDbConnection connection)
        {
            _connection = connection;
        }

        // Implementing the ExecuteReaderAsync method to match the interface
        public async Task<IDataReader> ExecuteReaderAsync(IDbCommand command, CancellationToken cancellationToken)
        {
            // Open the connection asynchronously
            await _connection.OpenAsync(cancellationToken);

            // Execute the reader asynchronously and return the IDataReader
            return await command.ExecuteReaderAsync(cancellationToken);
        }
    }
}
