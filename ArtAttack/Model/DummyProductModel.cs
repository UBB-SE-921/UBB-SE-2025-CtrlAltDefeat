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
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        }

        /// <summary>
        /// Adds a new DummyProduct record to the database
        /// </summary>
        /// <param name="name">Name of the product to be added</param>
        /// <param name="price">Price of the product to be added</param>
        /// <param name="sellerId">Id of the product's seller</param>
        /// <param name="productType">Type of the product</param>
        /// <param name="startDate">Beginning date for the product</param>
        /// <param name="endDate">End date for the product</param>
        /// <returns></returns>
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


        /// <summary>
        /// Updates an existing DummyProduct record in the database
        /// </summary>
        /// <param name="id">Id of the Product to be updated</param>
        /// <param name="name">Name to be updated</param>
        /// <param name="price">Price to be updated</param>
        /// <param name="sellerId">Seller Id to be updated</param>
        /// <param name="productType">Product type to be updated</param>
        /// <param name="startDate">Start date to be updated</param>
        /// <param name="endDate">End date to be updated</param>
        /// <returns></returns>
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

        /// <summary>
        /// Deletes a DummyProduct record from the database
        /// </summary>
        /// <param name="id">Id of the product to be deleted</param>
        /// <returns></returns>
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

        /// <summary>
        /// Retrieves a Seler's name by its ID.
        /// </summary>
        /// <param name="sellerId">Seller Id for which to retrieve name</param>
        /// <returns> string or null.</returns>
        internal async Task<string?> GetSellerNameAsync(int? sellerId)
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

        /// <summary>
        /// Retrieves a DummyProduct by its ID.
        /// </summary>
        /// <param name="productId">Product Id of the product to be fetched</param>
        /// <returns> string or null.</returns>
        internal async Task<DummyProduct> GetDummyProductByIdAsync(int productId)
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
        /// Retrieves a Seler's name by its ID
        /// </summary>
        /// <param name="sellerId">Id of the seller to be retrieved</param>
        /// <returns></returns>
        Task<string> IDummyProductModel.GetSellerNameAsync(int? sellerId)
        {
            return GetSellerNameAsync(sellerId);
        }

        /// <summary>
        /// Retrieves a DummyProduct by its ID
        /// </summary>
        /// <param name="productId">Id of the product to be retrieved</param>
        /// <returns></returns>
        Task<DummyProduct> IDummyProductModel.GetDummyProductByIdAsync(int productId)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }
}
