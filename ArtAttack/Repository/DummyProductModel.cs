using System;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Repository
{
    /// <summary>
    /// Provides database operations for dummy product management.
    /// </summary>
    public class DummyProductRepository : IDummyProductRepository
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyProductRepository"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public DummyProductRepository(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyProductRepository"/> class with a specified database provider.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="databaseProvider">The database provider to use.</param>
        public DummyProductRepository(string connectionString, IDatabaseProvider databaseProvider)
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

        /// <inheritdoc/>
        public async Task AddDummyProductAsync(string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "AddDummyProduct";
                    databaseCommand.CommandType = CommandType.StoredProcedure;

                    AddParameter(databaseCommand, "@Name", name);
                    AddParameter(databaseCommand, "@Price", price);
                    AddParameter(databaseCommand, "@SellerID", sellerId);
                    AddParameter(databaseCommand, "@ProductType", productType);
                    AddParameter(databaseCommand, "@StartDate", startDate);
                    AddParameter(databaseCommand, "@EndDate", endDate);

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
                }
            }
        }

        /// <inheritdoc/>
        public async Task UpdateDummyProductAsync(int id, string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "UpdateDummyProduct";
                    databaseCommand.CommandType = CommandType.StoredProcedure;

                    AddParameter(databaseCommand, "@ID", id);
                    AddParameter(databaseCommand, "@Name", name);
                    AddParameter(databaseCommand, "@Price", price);
                    AddParameter(databaseCommand, "@SellerID", sellerId);
                    AddParameter(databaseCommand, "@ProductType", productType);
                    AddParameter(databaseCommand, "@StartDate", startDate);
                    AddParameter(databaseCommand, "@EndDate", endDate);

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
                }
            }
        }

        /// <inheritdoc/>
        public async Task DeleteDummyProduct(int id)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "DeleteDummyProduct";
                    databaseCommand.CommandType = CommandType.StoredProcedure;

                    AddParameter(databaseCommand, "@ID", id);

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
                }
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "GetSellerById";
                    command.CommandType = CommandType.StoredProcedure;

                    if (sellerId.HasValue)
                    {
                        AddParameter(command, "@SellerID", sellerId.Value);
                    }
                    else
                    {
                        AddParameter(command, "@SellerID", DBNull.Value);
                    }

                    await connection.OpenAsync();

                    object result = await command.ExecuteScalarAsync();
                    return result?.ToString();
                }
            }
        }

        /// <inheritdoc/>
        public async Task<DummyProduct> GetDummyProductByIdAsync(int productId)
        {
            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "GetDummyProductByID";
                    command.CommandType = CommandType.StoredProcedure;

                    AddParameter(command, "@productID", productId);

                    await connection.OpenAsync();

                    using (IDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new DummyProduct
                            {
                                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Price = (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                SellerID = reader.IsDBNull(reader.GetOrdinal("SellerID")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("SellerID")),
                                ProductType = reader.GetString(reader.GetOrdinal("ProductType")),
                                StartDate = reader.IsDBNull(reader.GetOrdinal("StartDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("EndDate"))
                            };
                        }
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to add a parameter to a command
        /// </summary>
        private void AddParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;

            if (value == null)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.Value = value;
            }

            command.Parameters.Add(parameter);
        }
    }
}