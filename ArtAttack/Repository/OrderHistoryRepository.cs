using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Repository
{
    /// <summary>
    /// Provides database operations for order history management.
    /// </summary>
    public class OrderHistoryRepository : IOrderHistoryRepository
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryRepository"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public OrderHistoryRepository(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryRepository"/> class with a specified database provider.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="databaseProvider">The database provider to use.</param>
        public OrderHistoryRepository(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        }

        /// <inheritdoc/>
        public async Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryId)
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
                    orderHistoryParameter.Value = orderHistoryId;
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