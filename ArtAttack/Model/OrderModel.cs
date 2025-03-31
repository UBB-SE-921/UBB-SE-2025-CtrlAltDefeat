
using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace ArtAttack.Model
{
    public class OrderModel
    {
        private readonly string _connectionString;
        public string ConnectionString => _connectionString;

        public OrderModel(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task AddOrderAsync(int productId, int buyerId, int productType, string paymentMethod, int orderSummaryId, DateTime orderDate)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("AddOrder", conn))
                {
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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("UpdateOrder", conn))
                {
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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("DeleteOrder", conn))
                {
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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("get_borrowed_order_history", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BuyerID", SqlDbType.Int).Value = buyerId;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32("OrderID"),

                                ProductID = reader.GetInt32("ProductID"),
                                BuyerID = reader.GetInt32("BuyerID"),
                                OrderSummaryID = reader.GetInt32("OrderSummaryID"),
                                OrderHistoryID = reader.GetInt32("OrderHistoryID"),
                                ProductType = reader.GetInt32("ProductType"),
                                PaymentMethod = reader.GetString("PaymentMethod"),
                                OrderDate = reader.GetDateTime("OrderDate")

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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("get_new_or_used_order_history", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BuyerID", SqlDbType.Int).Value = buyerId;
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32("OrderID"),

                                ProductID = reader.GetInt32("ProductID"),
                                BuyerID = reader.GetInt32("BuyerID"),
                                OrderSummaryID = reader.GetInt32("OrderSummaryID"),
                                OrderHistoryID = reader.GetInt32("OrderHistoryID"),
                                ProductType = reader.GetInt32("ProductType"),
                                PaymentMethod = reader.GetString("PaymentMethod"),
                                OrderDate = reader.GetDateTime("OrderDate")
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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("get_orders_from_last_3_months", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BuyerID", SqlDbType.Int).Value = buyerId;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32("OrderID"),

                                ProductID = reader.GetInt32("ProductID"),
                                BuyerID = reader.GetInt32("BuyerID"),
                                OrderSummaryID = reader.GetInt32("OrderSummaryID"),
                                OrderHistoryID = reader.GetInt32("OrderHistoryID"),
                                ProductType = reader.GetInt32("ProductType"),
                                PaymentMethod = reader.GetString("PaymentMethod"),
                                OrderDate = reader.GetDateTime("OrderDate")
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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("get_orders_from_last_6_months", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BuyerID", SqlDbType.Int).Value = buyerId;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32("OrderID"),

                                ProductID = reader.GetInt32("ProductID"),
                                BuyerID = reader.GetInt32("BuyerID"),
                                OrderSummaryID = reader.GetInt32("OrderSummaryID"),
                                OrderHistoryID = reader.GetInt32("OrderHistoryID"),
                                ProductType = reader.GetInt32("ProductType"),
                                PaymentMethod = reader.GetString("PaymentMethod"),
                                OrderDate = reader.GetDateTime("OrderDate")
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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("get_orders_from_2025", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BuyerID", SqlDbType.Int).Value = buyerId;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32("OrderID"),

                                ProductID = reader.GetInt32("ProductID"),
                                BuyerID = reader.GetInt32("BuyerID"),
                                OrderSummaryID = reader.GetInt32("OrderSummaryID"),
                                OrderHistoryID = reader.GetInt32("OrderHistoryID"),
                                ProductType = reader.GetInt32("ProductType"),
                                PaymentMethod = reader.GetString("PaymentMethod"),
                                OrderDate = reader.GetDateTime("OrderDate")
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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("get_orders_from_2024", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BuyerID", SqlDbType.Int).Value = buyerId;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32("OrderID"),

                                ProductID = reader.GetInt32("ProductID"),
                                BuyerID = reader.GetInt32("BuyerID"),
                                OrderSummaryID = reader.GetInt32("OrderSummaryID"),
                                OrderHistoryID = reader.GetInt32("OrderHistoryID"),
                                ProductType = reader.GetInt32("ProductType"),
                                PaymentMethod = reader.GetString("PaymentMethod"),
                                OrderDate = reader.GetDateTime("OrderDate")
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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("get_orders_by_name", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BuyerID", SqlDbType.Int).Value = buyerId;
                    cmd.Parameters.Add("@text", SqlDbType.NVarChar, 250).Value = text;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32("OrderID"),
                                ProductID = reader.GetInt32("ProductID"),


                                BuyerID = reader.GetInt32("BuyerID"),
                                OrderSummaryID = reader.GetInt32("OrderSummaryID"),
                                OrderHistoryID = reader.GetInt32("OrderHistoryID"),
                                ProductType = reader.GetInt32("ProductType"),
                                PaymentMethod = reader.GetString("PaymentMethod"),
                                OrderDate = reader.GetDateTime("OrderDate")
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

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("get_orders_from_order_history", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@OrderHistoryID", SqlDbType.Int).Value = orderHistoryId;
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Order order = new Order()
                            {
                                OrderID = reader.GetInt32("OrderID"),

                                ProductID = reader.GetInt32("ProductID"),
                                BuyerID = reader.GetInt32("BuyerID"),
                                OrderSummaryID = reader.GetInt32("OrderSummaryID"),
                                OrderHistoryID = reader.GetInt32("OrderHistoryID"),
                                ProductType = reader.GetInt32("ProductType"),
                                PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? string.Empty : reader.GetString("PaymentMethod"),
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
