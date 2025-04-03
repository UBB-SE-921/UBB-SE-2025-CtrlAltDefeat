using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Shared;

namespace ArtAttack.Model
{
    public class OrderHistoryModel : IOrderHistoryModel
    {
        private readonly string _connectionString;
        private readonly IDatabaseProvider _databaseProvider;

        /// <summary>
        /// Default constructor for OrderHistoryModel.
        /// </summary>
        /// <param name="connectionString">The database connection string. Cannot be null or empty.</param>
        /// <remarks>
        /// Initializes a new instance of the OrderHistoryModel class with the specified connection string 
        /// and a default SqlDatabaseProvider. This constructor is typically used in production code.
        /// </remarks>
        public OrderHistoryModel(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Constructor for OrderHistoryModel with dependency injection support.
        /// </summary>
        /// <param name="connectionString">The database connection string. Cannot be null or empty.</param>
        /// <param name="databaseProvider">The database provider implementation to use for database operations. Cannot be null.</param>
        /// <remarks>
        /// Initializes a new instance of the OrderHistoryModel class with the specified connection string
        /// and database provider. This constructor is primarily used for testing with mock database providers.
        /// </remarks>
        public OrderHistoryModel(string connectionString, IDatabaseProvider databaseProvider)
        {
            _connectionString = connectionString;
            _databaseProvider = databaseProvider;
        }

        /// <summary>
        /// Retrieves a list of DummyProduct objects from the order history.
        /// </summary>
        /// <param name="orderHistoryID">The ID of the order history. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of DummyProduct objects.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public async Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID)
        {
            List<DummyProduct> dummyProducts = new List<DummyProduct>();

            using (IDbConnection connection = _databaseProvider.CreateConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "GetDummyProductsFromOrderHistory";
                    command.CommandType = CommandType.StoredProcedure;

                    IDbDataParameter param = command.CreateParameter();
                    param.ParameterName = "@OrderHistory";
                    param.Value = orderHistoryID;
                    command.Parameters.Add(param);

                    using (IDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            DummyProduct dummyProduct = new DummyProduct
                            {
                                ID = reader.GetInt32(reader.GetOrdinal("productID")),
                                Name = reader.GetString(reader.GetOrdinal("name")),
                                Price = (float)reader.GetDouble(reader.GetOrdinal("price")),
                                ProductType = reader.GetString(reader.GetOrdinal("productType")),
                                SellerID = reader["SellerID"] != DBNull.Value
                                    ? reader.GetInt32(reader.GetOrdinal("SellerID"))
                                    : 0,
                                StartDate = reader["startDate"] != DBNull.Value
                                    ? reader.GetDateTime(reader.GetOrdinal("startDate"))
                                    : DateTime.MinValue,
                                EndDate = reader["endDate"] != DBNull.Value
                                    ? reader.GetDateTime(reader.GetOrdinal("endDate"))
                                    : DateTime.MaxValue
                            };
                            dummyProducts.Add(dummyProduct);
                        }
                    }
                }
            }

            return dummyProducts;
        }
    }
}