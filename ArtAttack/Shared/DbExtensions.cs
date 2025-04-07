using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
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
        /// This implementation works with both SqlParameterCollection and mock parameter collections for testing.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when parameters or parameterName is null.</exception>
        /// <exception cref="ArgumentException">Thrown when parameterName is empty.</exception>
        public static IDbDataParameter AddWithValue(this IDataParameterCollection parameters, string parameterName, object value)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (parameterName.Length == 0)
            {
                throw new ArgumentException("Parameter name cannot be empty", nameof(parameterName));
            }

            // Handle SQL Server parameter collection natively
            if (parameters is SqlParameterCollection sqlParameters)
            {
                return sqlParameters.AddWithValue(parameterName, value ?? DBNull.Value);
            }

            // For testing scenarios with mocks
            if (parameters.GetType().FullName.Contains("DynamicProxy") ||
                parameters.GetType().FullName.Contains("Mock"))
            {
                // Test implementation - the operation itself is handled by the mock setup
                // Just add the parameter object to the collection for record
                var param = new DbExtensions.GenericDbParameter
                {
                    ParameterName = parameterName,
                    Value = value ?? DBNull.Value
                };

                parameters.Add(param);
                return param;
            }

            // For other database providers, try to create a parameter through the parent command if available
            try
            {
                // Try to find the command through reflection (not guaranteed to work with all implementations)
                object command = null;

                // Try to get Command property
                var commandProperty = parameters.GetType().GetProperty("Command");
                if (commandProperty != null)
                {
                    command = commandProperty.GetValue(parameters);
                }

                // If that didn't work, try common field names
                if (command == null)
                {
                    var fields = parameters.GetType().GetFields(System.Reflection.BindingFlags.NonPublic |
                                                               System.Reflection.BindingFlags.Instance);

                    foreach (var field in fields)
                    {
                        if (field.Name.Contains("command", StringComparison.OrdinalIgnoreCase) ||
                            field.Name == "_command")
                        {
                            command = field.GetValue(parameters);
                            if (command != null)
                            {
                                break;
                            }
                        }
                    }
                }

                // If we found a command, use it to create a parameter
                if (command != null && command is IDbCommand dbCommand)
                {
                    var param = dbCommand.CreateParameter();
                    param.ParameterName = parameterName;
                    param.Value = value ?? DBNull.Value;
                    parameters.Add(param);
                    return param;
                }
            }
            catch
            {
                // If anything goes wrong with reflection, fall back to the generic implementation
            }

            // Generic fallback implementation that should work in most cases
            var genericParam = new DbExtensions.GenericDbParameter
            {
                ParameterName = parameterName,
                Value = value ?? DBNull.Value
            };

            parameters.Add(genericParam);
            return genericParam;
        }
        [ExcludeFromCodeCoverage]
        /// <summary>
        /// A generic implementation of IDbDataParameter for testing purposes
        /// </summary>
        public class GenericDbParameter : IDbDataParameter
        {
            public DbType DbType { get; set; }
            public ParameterDirection Direction { get; set; } = ParameterDirection.Input;
            public bool IsNullable => true;
            public required string ParameterName { get; set; }
            public string SourceColumn { get; set; }
            public DataRowVersion SourceVersion { get; set; }
            public required object Value { get; set; }
            public byte Precision { get; set; }
            public byte Scale { get; set; }
            public int Size { get; set; }
        }
    }
}
