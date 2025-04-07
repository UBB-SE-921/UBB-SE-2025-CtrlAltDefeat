using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Model
{
    public class DummyProductModel : IDummyProductModel
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        [ExcludeFromCodeCoverage]
        public DummyProductModel(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        public DummyProductModel(string connectionString, IDatabaseProvider databaseProvider)
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
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "AddDummyProduct";
                    cmd.CommandType = CommandType.StoredProcedure;

                    AddParameter(cmd, "@Name", name);
                    AddParameter(cmd, "@Price", price);
                    AddParameter(cmd, "@SellerID", sellerId);
                    AddParameter(cmd, "@ProductType", productType);
                    AddParameter(cmd, "@StartDate", startDate);
                    AddParameter(cmd, "@EndDate", endDate);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateDummyProductAsync(int id, string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UpdateDummyProduct";
                    cmd.CommandType = CommandType.StoredProcedure;

                    AddParameter(cmd, "@ID", id);
                    AddParameter(cmd, "@Name", name);
                    AddParameter(cmd, "@Price", price);
                    AddParameter(cmd, "@SellerID", sellerId);
                    AddParameter(cmd, "@ProductType", productType);
                    AddParameter(cmd, "@StartDate", startDate);
                    AddParameter(cmd, "@EndDate", endDate);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteDummyProduct(int id)
        {
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DeleteDummyProduct";
                    cmd.CommandType = CommandType.StoredProcedure;

                    AddParameter(cmd, "@ID", id);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
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