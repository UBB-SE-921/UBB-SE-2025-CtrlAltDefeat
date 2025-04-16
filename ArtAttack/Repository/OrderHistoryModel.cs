using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using ArtAttack.Shared;

namespace ArtAttack.Model
{
    public class OrderHistoryModel : IOrderHistoryModel
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

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
            this.connectionString = connectionString;
            this.databaseProvider = databaseProvider;
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

            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                await connection.OpenAsync();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "GetDummyProductsFromOrderHistory";
                    command.CommandType = CommandType.StoredProcedure;

                    IDbDataParameter orderHistoryParameter = command.CreateParameter();
                    orderHistoryParameter.ParameterName = "@OrderHistory";
                    orderHistoryParameter.Value = orderHistoryID;
                    command.Parameters.Add(orderHistoryParameter);

                    using (IDataReader dataReader = await command.ExecuteReaderAsync())
                    {
                        while (await dataReader.ReadAsync())
                        {
                            DummyProduct dummyProduct = new DummyProduct();

                            dummyProduct.ID = dataReader.GetInt32(dataReader.GetOrdinal("productID"));
                            dummyProduct.Name = dataReader.GetString(dataReader.GetOrdinal("name"));
                            dummyProduct.Price = (float)dataReader.GetDouble(dataReader.GetOrdinal("price"));
                            dummyProduct.ProductType = dataReader.GetString(dataReader.GetOrdinal("productType"));

                            if (dataReader["SellerID"] == DBNull.Value)
                            {
                                dummyProduct.SellerID = 0;
                            }
                            else
                            {
                                dummyProduct.SellerID = dataReader.GetInt32(dataReader.GetOrdinal("SellerID"));
                            }

                            if (dataReader["startDate"] == DBNull.Value)
                            {
                                dummyProduct.StartDate = DateTime.MinValue;
                            }
                            else
                            {
                                dummyProduct.StartDate = dataReader.GetDateTime(dataReader.GetOrdinal("startDate"));
                            }

                            if (dataReader["endDate"] == DBNull.Value)
                            {
                                dummyProduct.EndDate = DateTime.MaxValue;
                            }
                            else
                            {
                                dummyProduct.EndDate = dataReader.GetDateTime(dataReader.GetOrdinal("endDate"));
                            }

                            dummyProducts.Add(dummyProduct);
                        }
                    }
                }
            }

            return dummyProducts;
        }
    }
}