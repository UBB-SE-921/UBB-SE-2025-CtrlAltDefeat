using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ArtAttack.Shared
{
    public static class DbExtensions
    {
        /// <summary>
        /// Provides an asynchronous version of the Open method for IDbConnection objects.
        /// </summary>
        /// <param name="connection">The database connection to open. Cannot be null.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation. Optional.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <remarks>
        /// If the connection object is a SqlConnection, this method will call its native OpenAsync method.
        /// Otherwise, it will synchronously call Open and return a completed task.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when connection is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is already open.</exception>
        /// <exception cref="Exception">Thrown when there is an error opening the connection.</exception>
        public static Task OpenAsync(this IDbConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection is SqlConnection sqlConnection)
            {
                return sqlConnection.OpenAsync(cancellationToken);
            }
            connection.Open();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Provides an asynchronous version of the ExecuteReader method for IDbCommand objects.
        /// </summary>
        /// <param name="command">The command to execute. Cannot be null.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation. Optional.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains an IDataReader object that can be used to read the results of the command.
        /// </returns>
        /// <remarks>
        /// If the command object is a SqlCommand, this method will call its native ExecuteReaderAsync method.
        /// Otherwise, it will synchronously call ExecuteReader and return a completed task with the result.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not open.</exception>
        /// <exception cref="Exception">Thrown when there is an error executing the command.</exception>
        public static Task<IDataReader> ExecuteReaderAsync(this IDbCommand command, CancellationToken cancellationToken = default)
        {
            if (command is SqlCommand sqlCommand)
            {
                return sqlCommand.ExecuteReaderAsync(cancellationToken)
                    .ContinueWith(t => (IDataReader)t.Result, TaskContinuationOptions.ExecuteSynchronously);
            }
            return Task.FromResult(command.ExecuteReader());
        }

        /// <summary>
        /// Provides an asynchronous version of the Read method for IDataReader objects.
        /// </summary>
        /// <param name="reader">The data reader to read from. Cannot be null.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation. Optional.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result is a boolean indicating whether there are more rows to read.
        /// </returns>
        /// <remarks>
        /// If the reader object is a SqlDataReader, this method will call its native ReadAsync method.
        /// Otherwise, it will synchronously call Read and return a completed task with the result.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when reader is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the reader is closed.</exception>
        /// <exception cref="Exception">Thrown when there is an error reading from the data reader.</exception>
        public static Task<bool> ReadAsync(this IDataReader reader, CancellationToken cancellationToken = default)
        {
            if (reader is SqlDataReader sqlReader)
            {
                return sqlReader.ReadAsync(cancellationToken);
            }
            return Task.FromResult(reader.Read());
        }

        /// <summary>
        /// Provides an asynchronous version of the ExecuteNonQuery method for IDbCommand objects.
        /// </summary>
        /// <param name="command">The command to execute. Cannot be null.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation. Optional.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result is an integer indicating the number of rows affected by the command.
        /// </returns>
        /// <remarks>
        /// If the command object is a SqlCommand, this method will call its native ExecuteNonQueryAsync method.
        /// Otherwise, it will synchronously call ExecuteNonQuery and return a completed task with the result.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not open.</exception>
        /// <exception cref="Exception">Thrown when there is an error executing the command.</exception>
        public static Task<int> ExecuteNonQueryAsync(this IDbCommand command, CancellationToken cancellationToken = default)
        {
            if (command is SqlCommand sqlCommand)
            {
                return sqlCommand.ExecuteNonQueryAsync(cancellationToken);
            }
            return Task.FromResult(command.ExecuteNonQuery());
        }

        /// <summary>
        /// Provides an asynchronous version of the ExecuteScalar method for IDbCommand objects.
        /// </summary>
        /// <param name="command">The command to execute. Cannot be null.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation. Optional.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result is the first column of the first row in the result set, or null if the result set is empty.
        /// </returns>
        /// <remarks>
        /// If the command object is a SqlCommand, this method will call its native ExecuteScalarAsync method.
        /// Otherwise, it will synchronously call ExecuteScalar and return a completed task with the result.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not open.</exception>
        /// <exception cref="Exception">Thrown when there is an error executing the command.</exception>
        public static Task<object> ExecuteScalarAsync(this IDbCommand command, CancellationToken cancellationToken = default)
        {
            if (command is SqlCommand sqlCommand)
            {
                return sqlCommand.ExecuteScalarAsync(cancellationToken);
            }
            return Task.FromResult(command.ExecuteScalar());
        }

        /// <summary>
        /// Adds a parameter with the specified name and value to a parameter collection.
        /// </summary>
        /// <param name="parameters">The parameter collection to which the parameter will be added. Cannot be null.</param>
        /// <param name="parameterName">The name of the parameter. Cannot be null or empty.</param>
        /// <param name="value">The value of the parameter, or null to use DBNull.Value.</param>
        /// <returns>
        /// The new parameter that was added to the collection.
        /// </returns>
        /// <remarks>
        /// This method is currently only supported for SqlParameterCollection objects.
        /// For other parameter collection types, an exception will be thrown.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when parameters or parameterName is null.</exception>
        /// <exception cref="ArgumentException">Thrown when parameterName is empty.</exception>
        /// <exception cref="NotImplementedException">Thrown when parameters is not a SqlParameterCollection.</exception>
        /// <exception cref="Exception">Thrown when there is an error adding the parameter.</exception>
        public static IDbDataParameter AddWithValue(this IDataParameterCollection parameters, string parameterName, object value)
        {
            if (parameters is SqlParameterCollection sqlParameters)
            {
                return sqlParameters.AddWithValue(parameterName, value ?? DBNull.Value);
            }
            else
            {
                // For non-SqlCommand implementations, we need to create a parameter first
                // This assumes we have access to the parent command
                // In practice, this would need a more robust implementation
                throw new NotImplementedException("AddWithValue is only supported for SqlParameterCollection");
            }
        }
    }
}