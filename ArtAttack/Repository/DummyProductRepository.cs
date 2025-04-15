using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Repository
{
    public class DummyProductRepository : IDummyProductRepository
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        [ExcludeFromCodeCoverage]
        public DummyProductRepository(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

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

        public async Task DeleteDummyProductAsync(int id)
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
                                SellerID = reader.IsDBNull(reader.GetOrdinal("SellerID")) ? null : reader.GetInt32(reader.GetOrdinal("SellerID")),
                                ProductType = reader.GetString(reader.GetOrdinal("ProductType")),
                                StartDate = reader.IsDBNull(reader.GetOrdinal("StartDate")) ? null : reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate")) ? null : reader.GetDateTime(reader.GetOrdinal("EndDate"))
                            };
                        }
                        return null;
                    }
                }
            }
        }

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