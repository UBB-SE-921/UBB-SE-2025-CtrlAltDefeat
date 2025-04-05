using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Model
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

        public async Task AddOrderAsync(int productId, int buyerId, int productType, string paymentMethod, int orderSummaryId, DateTime orderDate)
        {
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "AddOrder";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductID", productId);
                    cmd.Parameters.AddWithValue("@BuyerID", buyerId);
                    cmd.Parameters.AddWithValue("@ProductType", productType);
                    cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                    cmd.Parameters.AddWithValue("@OrderSummaryID", orderSummaryId);
                    cmd.Parameters.AddWithValue("@OrderDate", orderDate);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateOrderAsync(int orderId, int productType, string paymentMethod, DateTime orderDate)
        {
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UpdateOrder";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    cmd.Parameters.AddWithValue("@ProductType", productType);
                    cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                    cmd.Parameters.AddWithValue("@OrderDate", orderDate);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DeleteOrder";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "get_borrowed_order_history";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BuyerID", buyerId);
                    await conn.OpenAsync();

                    using (IDataReader reader = await cmd.ExecuteReaderAsync())
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

        public async Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "get_new_or_used_order_history";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BuyerID", buyerId);
                    await conn.OpenAsync();

                    using (IDataReader reader = await cmd.ExecuteReaderAsync())
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

        public List<Order> GetOrdersFromLastThreeMonths(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "get_orders_from_last_3_months";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BuyerID", buyerId);
                    conn.Open();

                    using (IDataReader reader = cmd.ExecuteReader())
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

        public List<Order> GetOrdersFromLastSixMonths(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "get_orders_from_last_6_months";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BuyerID", buyerId);
                    conn.Open();

                    using (IDataReader reader = cmd.ExecuteReader())
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

        public List<Order> GetOrdersFrom2025(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "get_orders_from_2025";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BuyerID", buyerId);
                    conn.Open();

                    using (IDataReader reader = cmd.ExecuteReader())
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

        public List<Order> GetOrdersFrom2024(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "get_orders_from_2024";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BuyerID", buyerId);
                    conn.Open();

                    using (IDataReader reader = cmd.ExecuteReader())
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

        public List<Order> GetOrdersByName(int buyerId, string text)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "get_orders_by_name";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BuyerID", buyerId);
                    cmd.Parameters.AddWithValue("@text", text);
                    conn.Open();

                    using (IDataReader reader = cmd.ExecuteReader())
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

        public async Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId)
        {
            List<Order> orders = new List<Order>();

            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "get_orders_from_order_history";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrderHistoryID", orderHistoryId);
                    await conn.OpenAsync();

                    using (IDataReader reader = await cmd.ExecuteReaderAsync())
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
