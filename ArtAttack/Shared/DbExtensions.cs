using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ArtAttack.Shared
{
    public static class DbExtensions
    {
        public static Task OpenAsync(this IDbConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection is SqlConnection sqlConnection)
            {
                return sqlConnection.OpenAsync(cancellationToken);
            }
            connection.Open();
            return Task.CompletedTask;
        }

        public static Task<IDataReader> ExecuteReaderAsync(this IDbCommand command, CancellationToken cancellationToken = default)
        {
            if (command is SqlCommand sqlCommand)
            {
                return sqlCommand.ExecuteReaderAsync(cancellationToken)
                    .ContinueWith(t => (IDataReader)t.Result, TaskContinuationOptions.ExecuteSynchronously);
            }
            return Task.FromResult(command.ExecuteReader());
        }

        public static Task<bool> ReadAsync(this IDataReader reader, CancellationToken cancellationToken = default)
        {
            if (reader is SqlDataReader sqlReader)
            {
                return sqlReader.ReadAsync(cancellationToken);
            }
            return Task.FromResult(reader.Read());
        }

        public static Task<int> ExecuteNonQueryAsync(this IDbCommand command, CancellationToken cancellationToken = default)
        {
            if (command is SqlCommand sqlCommand)
            {
                return sqlCommand.ExecuteNonQueryAsync(cancellationToken);
            }
            return Task.FromResult(command.ExecuteNonQuery());
        }

        public static Task<object> ExecuteScalarAsync(this IDbCommand command, CancellationToken cancellationToken = default)
        {
            if (command is SqlCommand sqlCommand)
            {
                return sqlCommand.ExecuteScalarAsync(cancellationToken);
            }
            return Task.FromResult(command.ExecuteScalar());
        }

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
