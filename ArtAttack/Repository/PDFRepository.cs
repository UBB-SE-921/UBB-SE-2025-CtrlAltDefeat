using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtAttack.Shared;

namespace ArtAttack.Repository
{
    public class PDFRepository : IPDFRepository
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        [ExcludeFromCodeCoverage]
        public PDFRepository(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the TrackedOrderRepository class
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="databaseProvider">Database provider for creating connections</param>
        /// <exception cref="ArgumentNullException">Thrown when connection string or provider is null</exception>
        public PDFRepository(string connectionString, IDatabaseProvider databaseProvider)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (databaseProvider == null)
            {
                throw new ArgumentNullException(nameof(databaseProvider));
            }

            this.connectionString = connectionString;
            this.databaseProvider = databaseProvider;
        }

        public async Task<int> InsertPdfAsync(byte[] fileBytes)
        {
            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO PDF ([file]) OUTPUT INSERTED.ID VALUES (@file)";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@file";
                parameter.Value = fileBytes;
                command.Parameters.Add(parameter);

                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }
    }
}
