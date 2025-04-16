using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Repository
{
    public class OrderModel : IOrderModel
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        public string ConnectionString => connectionString;

        public OrderModel(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        public OrderModel(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString;
            this.databaseProvider = databaseProvider;
        }

        /// <summary>
        /// Adds an order to the database using the AddOrder stored procedure
        /// </summary>
        /// <param name="productId">Product of the order to be added</param>
        /// <param name="buyerId">Id of the buyer to be added to the order</param>
        /// <param name="productType">Product type of the order to be added</param>
        /// <param name="paymentMethod">The payment method of the order to be added</param>
        /// <param name="orderSummaryId">The order to be added's summary id</param>
        /// <param name="orderDate">The date of the order to be added</param>
        /// <returns></returns>
        public async Task AddOrderAsync(int productId, int buyerId, int productType, string paymentMethod, int orderSummaryId, DateTime orderDate)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "AddOrder";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@ProductID", productId);
                    databaseCommand.Parameters.AddWithValue("@BuyerID", buyerId);
                    databaseCommand.Parameters.AddWithValue("@ProductType", productType);
                    databaseCommand.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                    databaseCommand.Parameters.AddWithValue("@OrderSummaryID", orderSummaryId);
                    databaseCommand.Parameters.AddWithValue("@OrderDate", orderDate);

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Updates a specific order in the database using the UpdateOrder stored procedure
        /// </summary>
        /// <param name="orderId">The Id of the order to be updated</param>
        /// <param name="productType">The type of the product to be updated</param>
        /// <param name="paymentMethod">The payment method to be updated</param>
        /// <param name="orderDate">The date of the order to be added</param>
        /// <returns></returns>
        public async Task UpdateOrderAsync(int orderId, int productType, string paymentMethod, DateTime orderDate)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "UpdateOrder";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@OrderID", orderId);
                    databaseCommand.Parameters.AddWithValue("@ProductType", productType);
                    databaseCommand.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                    databaseCommand.Parameters.AddWithValue("@OrderDate", orderDate);

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Deletes a specific order from the database using the DeleteOrder stored procedure
        /// </summary>
        /// <param name="orderId">The Id of the order to delete</param>
        /// <returns></returns>
        public async Task DeleteOrderAsync(int orderId)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "DeleteOrder";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@OrderID", orderId);

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Retrieves the order history of a specific buyer using the get_borrowed_order_history stored procedure
        /// </summary>
        /// <param name="buyerId">The buyer's id for which we want to retrieve the history of</param>
        /// <returns></returns>
        public async Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "get_borrowed_order_history";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@BuyerID", buyerId);
                    await databaseConnection.OpenAsync();

                    using (IDataReader reader = await databaseCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                BuyerID = reader.GetInt32(reader.GetOrdinal("BuyerID")),
                                OrderSummaryID = reader.GetInt32(reader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = reader.GetInt32(reader.GetOrdinal("OrderHistoryID")),
                                ProductType = reader.GetInt32(reader.GetOrdinal("ProductType")),
                                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate"))
                            };
                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }

        /// <summary>
        /// Retrieves the order history of a specific buyer using the get_new_or_used_order_history stored procedure
        /// </summary>
        /// <param name="buyerId">The id of the buyer we wish to retrieve the lists of orders from</param>
        /// <returns></returns>
        public async Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "get_new_or_used_order_history";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@BuyerID", buyerId);
                    await databaseConnection.OpenAsync();

                    using (IDataReader reader = await databaseCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                BuyerID = reader.GetInt32(reader.GetOrdinal("BuyerID")),
                                OrderSummaryID = reader.GetInt32(reader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = reader.GetInt32(reader.GetOrdinal("OrderHistoryID")),
                                ProductType = reader.GetInt32(reader.GetOrdinal("ProductType")),
                                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate"))
                            };
                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }

        /// <summary>
        /// Retrieves the orders of a specific buyer from the last three months using the get_orders_from_last_3_months stored procedure
        /// </summary>
        /// <param name="buyerId">The Id of the buyer for which we want to retrieve the orders of the last 3 months</param>
        /// <returns></returns>
        public List<Order> GetOrdersFromLastThreeMonths(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "get_orders_from_last_3_months";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@BuyerID", buyerId);
                    databaseConnection.Open();

                    using (IDataReader reader = databaseCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                BuyerID = reader.GetInt32(reader.GetOrdinal("BuyerID")),
                                OrderSummaryID = reader.GetInt32(reader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = reader.GetInt32(reader.GetOrdinal("OrderHistoryID")),
                                ProductType = reader.GetInt32(reader.GetOrdinal("ProductType")),
                                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate"))
                            };
                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }

        /// <summary>
        /// Retrieves the orders of a specific buyer from the last six months using the get_orders_from_last_6_months stored procedure
        /// </summary>
        /// <param name="buyerId">The Id of the buyer for which we want to retrieve the orders of the last 6 months</param>
        /// <returns></returns>
        public List<Order> GetOrdersFromLastSixMonths(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "get_orders_from_last_6_months";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@BuyerID", buyerId);
                    databaseConnection.Open();

                    using (IDataReader reader = databaseCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                BuyerID = reader.GetInt32(reader.GetOrdinal("BuyerID")),
                                OrderSummaryID = reader.GetInt32(reader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = reader.GetInt32(reader.GetOrdinal("OrderHistoryID")),
                                ProductType = reader.GetInt32(reader.GetOrdinal("ProductType")),
                                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate"))
                            };
                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }

        /// <summary>
        /// Retrieves the orders of a specific buyer from 2025 using the get_orders_from_2025 stored procedure
        /// </summary>
        /// <param name="buyerId">The buyer's id for which we want to retrieve the orders from 2025</param>
        /// <returns></returns>
        public List<Order> GetOrdersFrom2025(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "get_orders_from_2025";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@BuyerID", buyerId);
                    databaseConnection.Open();

                    using (IDataReader reader = databaseCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                BuyerID = reader.GetInt32(reader.GetOrdinal("BuyerID")),
                                OrderSummaryID = reader.GetInt32(reader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = reader.GetInt32(reader.GetOrdinal("OrderHistoryID")),
                                ProductType = reader.GetInt32(reader.GetOrdinal("ProductType")),
                                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate"))
                            };
                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }

        /// <summary>
        /// Retrieves the orders of a specific buyer from 2024 using the get_orders_from_2024 stored procedure
        /// </summary>
        /// <param name="buyerId">The Id of the buyer for which we wish to retrieve the orders of</param>
        /// <returns></returns>
        public List<Order> GetOrdersFrom2024(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "get_orders_from_2024";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@BuyerID", buyerId);
                    databaseConnection.Open();

                    using (IDataReader reader = databaseCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                BuyerID = reader.GetInt32(reader.GetOrdinal("BuyerID")),
                                OrderSummaryID = reader.GetInt32(reader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = reader.GetInt32(reader.GetOrdinal("OrderHistoryID")),
                                ProductType = reader.GetInt32(reader.GetOrdinal("ProductType")),
                                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate"))
                            };
                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }

        /// <summary>
        /// Retrieves the orders of a specific buyer using the get_orders_by_name stored procedure
        /// </summary>
        /// <param name="buyerId">The id of the buyer for which to get the orders</param>
        /// <param name="text">The text which contains all of the order's names</param>
        /// <returns></returns>
        public List<Order> GetOrdersByName(int buyerId, string text)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "get_orders_by_name";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@BuyerID", buyerId);
                    databaseCommand.Parameters.AddWithValue("@text", text);
                    databaseConnection.Open();

                    using (IDataReader reader = databaseCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                BuyerID = reader.GetInt32(reader.GetOrdinal("BuyerID")),
                                OrderSummaryID = reader.GetInt32(reader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = reader.GetInt32(reader.GetOrdinal("OrderHistoryID")),
                                ProductType = reader.GetInt32(reader.GetOrdinal("ProductType")),
                                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate"))
                            };
                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }

        /// <summary>
        /// Retrieves the orders from a specific order history using the get_orders_from_order_history stored procedure
        /// </summary>
        /// <param name="orderHistoryId">The order history ID for which to retrieve list of orders</param>
        /// <returns></returns>
        public async Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId)
        {
            List<Order> orders = new List<Order>();

            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "get_orders_from_order_history";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@OrderHistoryID", orderHistoryId);
                    await databaseConnection.OpenAsync();

                    using (IDataReader reader = await databaseCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                BuyerID = reader.GetInt32(reader.GetOrdinal("BuyerID")),
                                OrderSummaryID = reader.GetInt32(reader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = reader.GetInt32(reader.GetOrdinal("OrderHistoryID")),
                                ProductType = reader.GetInt32(reader.GetOrdinal("ProductType")),
                                PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? string.Empty : reader.GetString(reader.GetOrdinal("PaymentMethod")),
                                OrderDate = reader.IsDBNull(reader.GetOrdinal("OrderDate")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("OrderDate"))
                            };
                            orders.Add(order);
                        }
                    }
                }
            }

            return orders;
        }
    }
}