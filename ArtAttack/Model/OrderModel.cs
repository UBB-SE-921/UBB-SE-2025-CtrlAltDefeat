
using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using ArtAttack.Shared;

namespace ArtAttack.Model
{
    public interface IOrderModel
    {
        Task AddOrderAsync(int productId, int buyerId, int productType, string paymentMethod, int orderSummaryId, DateTime orderDate);
        Task UpdateOrderAsync(int orderId, int productType, string paymentMethod, DateTime orderDate);
        Task DeleteOrderAsync(int orderId);
        Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId);
        Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId);
        List<Order> GetOrdersFromLastThreeMonths(int buyerId);
        List<Order> GetOrdersFromLastSixMonths(int buyerId);
        List<Order> GetOrdersFrom2025(int buyerId);
        List<Order> GetOrdersFrom2024(int buyerId);
        List<Order> GetOrdersByName(int buyerId, string searchText);
        Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId);
    }
    public class OrderModel : IOrderModel
    {
        private readonly string _connectionString;
        private readonly IDatabaseProvider _databaseProvider;

        /// <summary>
        /// Gets the database connection string
        /// </summary>
        public string ConnectionString => _connectionString;

        /// <summary>
        /// Initializes a new instance of the OrderModel class
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="databaseProvider">Database provider for creating connections</param>
        public OrderModel(string connectionString, IDatabaseProvider databaseProvider)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Adds a new order to the database
        /// </summary>
        /// <param name="productId">ID of the product being ordered</param>
        /// <param name="buyerId">ID of the buyer placing the order</param>
        /// <param name="productType">Type of the product (1: New, 2: Used, 3: Borrowed)</param>
        /// <param name="paymentMethod">Method of payment (e.g., "Credit Card", "PayPal")</param>
        /// <param name="orderSummaryId">ID of the order summary</param>
        /// <param name="orderDate">Date and time when the order was placed</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task AddOrderAsync(int productId, int buyerId, int productType, string paymentMethod, int orderSummaryId, DateTime orderDate)
        {
            using (IDbConnection connection = _databaseProvider.CreateConnection(_connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "AddOrder";
                    command.CommandType = CommandType.StoredProcedure;
                    AddParameter(command, "@ProductID", productId);
                    AddParameter(command, "@BuyerID", buyerId);
                    AddParameter(command, "@ProductType", productType);
                    AddParameter(command, "@PaymentMethod", paymentMethod);
                    AddParameter(command, "@OrderSummaryID", orderSummaryId);
                    AddParameter(command, "@OrderDate", orderDate);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Updates an existing order in the database
        /// </summary>
        /// <param name="orderId">ID of the order to update</param>
        /// <param name="productType">New product type value</param>
        /// <param name="paymentMethod">New payment method value</param>
        /// <param name="orderDate">New order date value</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task UpdateOrderAsync(int orderId, int productType, string paymentMethod, DateTime orderDate)
        {
            using (IDbConnection connection = _databaseProvider.CreateConnection(_connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UpdateOrder";
                    command.CommandType = CommandType.StoredProcedure;
                    AddParameter(command, "@OrderID", orderId);
                    AddParameter(command, "@ProductType", productType);
                    AddParameter(command, "@PaymentMethod", paymentMethod);
                    AddParameter(command, "@OrderDate", orderDate);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Removes an order from the database
        /// </summary>
        /// <param name="orderId">ID of the order to delete</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task DeleteOrderAsync(int orderId)
        {
            using (IDbConnection connection = _databaseProvider.CreateConnection(_connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DeleteOrder";
                    command.CommandType = CommandType.StoredProcedure;
                    AddParameter(command, "@OrderID", orderId);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Retrieves all borrowed items ordered by a specific buyer
        /// </summary>
        /// <param name="buyerId">ID of the buyer</param>
        /// <returns>List of borrowed orders for the specified buyer</returns>
        public async Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId)
        {
            List<Order> orderList = new List<Order>();
            using (IDbConnection connection = _databaseProvider.CreateConnection(_connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "get_borrowed_order_history";
                    command.CommandType = CommandType.StoredProcedure;
                    AddParameter(command, "@BuyerID", buyerId);
                    await connection.OpenAsync();

                    using (IDataReader dataReader = await command.ExecuteReaderAsync())
                    {
                        while (await dataReader.ReadAsync())
                        {
                            Order orderItem = new Order()
                            {
                                OrderID = dataReader.GetInt32(dataReader.GetOrdinal("OrderID")),
                                ProductID = dataReader.GetInt32(dataReader.GetOrdinal("ProductID")),
                                BuyerID = dataReader.GetInt32(dataReader.GetOrdinal("BuyerID")),
                                OrderSummaryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderHistoryID")),
                                ProductType = dataReader.GetInt32(dataReader.GetOrdinal("ProductType")),
                                PaymentMethod = dataReader.GetString(dataReader.GetOrdinal("PaymentMethod")),
                                OrderDate = dataReader.GetDateTime(dataReader.GetOrdinal("OrderDate"))
                            };
                            orderList.Add(orderItem);
                        }
                    }
                }
            }
            return orderList;
        }

        /// <summary>
        /// Retrieves all new or used items ordered by a specific buyer
        /// </summary>
        /// <param name="buyerId">ID of the buyer</param>
        /// <returns>List of new or used orders for the specified buyer</returns>
        public async Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId)
        {
            List<Order> orderList = new List<Order>();
            using (IDbConnection connection = _databaseProvider.CreateConnection(_connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "get_new_or_used_order_history";
                    command.CommandType = CommandType.StoredProcedure;
                    AddParameter(command, "@BuyerID", buyerId);
                    await connection.OpenAsync();

                    using (IDataReader dataReader = await command.ExecuteReaderAsync())
                    {
                        while (await dataReader.ReadAsync())
                        {
                            Order orderItem = new Order()
                            {
                                OrderID = dataReader.GetInt32(dataReader.GetOrdinal("OrderID")),
                                ProductID = dataReader.GetInt32(dataReader.GetOrdinal("ProductID")),
                                BuyerID = dataReader.GetInt32(dataReader.GetOrdinal("BuyerID")),
                                OrderSummaryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderHistoryID")),
                                ProductType = dataReader.GetInt32(dataReader.GetOrdinal("ProductType")),
                                PaymentMethod = dataReader.GetString(dataReader.GetOrdinal("PaymentMethod")),
                                OrderDate = dataReader.GetDateTime(dataReader.GetOrdinal("OrderDate"))
                            };
                            orderList.Add(orderItem);
                        }
                    }
                }
            }
            return orderList;
        }


        public List<Order> GetOrdersFromLastThreeMonths(int buyerId)
        {
            List<Order> orderList = new List<Order>();
            using (IDbConnection connection = _databaseProvider.CreateConnection(_connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "get_orders_from_last_3_months";
                    command.CommandType = CommandType.StoredProcedure;
                    AddParameter(command, "@BuyerID", buyerId);
                    connection.Open();

                    using (IDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            Order orderItem = new Order()
                            {
                                OrderID = dataReader.GetInt32(dataReader.GetOrdinal("OrderID")),
                                ProductID = dataReader.GetInt32(dataReader.GetOrdinal("ProductID")),
                                BuyerID = dataReader.GetInt32(dataReader.GetOrdinal("BuyerID")),
                                OrderSummaryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderHistoryID")),
                                ProductType = dataReader.GetInt32(dataReader.GetOrdinal("ProductType")),
                                PaymentMethod = dataReader.GetString(dataReader.GetOrdinal("PaymentMethod")),
                                OrderDate = dataReader.GetDateTime(dataReader.GetOrdinal("OrderDate"))
                            };
                            orderList.Add(orderItem);
                        }
                    }
                }
            }
            return orderList;
        }

        /// <summary>
        /// Gets all orders placed by a buyer within the last six months
        /// </summary>
        /// <param name="buyerId">ID of the buyer</param>
        /// <returns>List of orders from the last six months</returns>
        public List<Order> GetOrdersFromLastSixMonths(int buyerId)
        {
            List<Order> orderList = new List<Order>();
            using (IDbConnection connection = _databaseProvider.CreateConnection(_connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "get_orders_from_last_6_months";
                    command.CommandType = CommandType.StoredProcedure;
                    AddParameter(command, "@BuyerID", buyerId);
                    connection.Open();

                    using (IDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            Order orderItem = new Order()
                            {
                                OrderID = dataReader.GetInt32(dataReader.GetOrdinal("OrderID")),
                                ProductID = dataReader.GetInt32(dataReader.GetOrdinal("ProductID")),
                                BuyerID = dataReader.GetInt32(dataReader.GetOrdinal("BuyerID")),
                                OrderSummaryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderHistoryID")),
                                ProductType = dataReader.GetInt32(dataReader.GetOrdinal("ProductType")),
                                PaymentMethod = dataReader.GetString(dataReader.GetOrdinal("PaymentMethod")),
                                OrderDate = dataReader.GetDateTime(dataReader.GetOrdinal("OrderDate"))
                            };
                            orderList.Add(orderItem);
                        }
                    }
                }
            }
            return orderList;
        }

        /// <summary>
        /// Gets all orders placed by a buyer in the year 2025
        /// </summary>
        /// <param name="buyerId">ID of the buyer</param>
        /// <returns>List of orders from 2025</returns>
        public List<Order> GetOrdersFrom2025(int buyerId)
        {
            List<Order> orderList = new List<Order>();
            using (IDbConnection connection = _databaseProvider.CreateConnection(_connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "get_orders_from_2025";
                    command.CommandType = CommandType.StoredProcedure;
                    AddParameter(command, "@BuyerID", buyerId);
                    connection.Open();

                    using (IDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            Order orderItem = new Order()
                            {
                                OrderID = dataReader.GetInt32(dataReader.GetOrdinal("OrderID")),
                                ProductID = dataReader.GetInt32(dataReader.GetOrdinal("ProductID")),
                                BuyerID = dataReader.GetInt32(dataReader.GetOrdinal("BuyerID")),
                                OrderSummaryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderHistoryID")),
                                ProductType = dataReader.GetInt32(dataReader.GetOrdinal("ProductType")),
                                PaymentMethod = dataReader.GetString(dataReader.GetOrdinal("PaymentMethod")),
                                OrderDate = dataReader.GetDateTime(dataReader.GetOrdinal("OrderDate"))
                            };
                            orderList.Add(orderItem);
                        }
                    }
                }
            }
            return orderList;
        }

        /// <summary>
        /// Gets all orders placed by a buyer in the year 2024
        /// </summary>
        /// <param name="buyerId">ID of the buyer</param>
        /// <returns>List of orders from 2024</returns>
        public List<Order> GetOrdersFrom2024(int buyerId)
        {
            List<Order> orderList = new List<Order>();
            using (IDbConnection connection = _databaseProvider.CreateConnection(_connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "get_orders_from_2024";
                    command.CommandType = CommandType.StoredProcedure;
                    AddParameter(command, "@BuyerID", buyerId);
                    connection.Open();

                    using (IDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            Order orderItem = new Order()
                            {
                                OrderID = dataReader.GetInt32(dataReader.GetOrdinal("OrderID")),
                                ProductID = dataReader.GetInt32(dataReader.GetOrdinal("ProductID")),
                                BuyerID = dataReader.GetInt32(dataReader.GetOrdinal("BuyerID")),
                                OrderSummaryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderHistoryID")),
                                ProductType = dataReader.GetInt32(dataReader.GetOrdinal("ProductType")),
                                PaymentMethod = dataReader.GetString(dataReader.GetOrdinal("PaymentMethod")),
                                OrderDate = dataReader.GetDateTime(dataReader.GetOrdinal("OrderDate"))
                            };
                            orderList.Add(orderItem);
                        }
                    }
                }
            }
            return orderList;
        }

        /// <summary>
        /// Searches for orders by product name for a specific buyer
        /// </summary>
        /// <param name="buyerId">ID of the buyer</param>
        /// <param name="searchText">Text to search for in product names</param>
        /// <returns>List of matching orders</returns>
        public List<Order> GetOrdersByName(int buyerId, string searchText)
        {
            List<Order> orderList = new List<Order>();
            using (IDbConnection connection = _databaseProvider.CreateConnection(_connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "get_orders_by_name";
                    command.CommandType = CommandType.StoredProcedure;
                    AddParameter(command, "@BuyerID", buyerId);
                    AddParameter(command, "@text", searchText);
                    connection.Open();

                    using (IDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            Order orderItem = new Order()
                            {
                                OrderID = dataReader.GetInt32(dataReader.GetOrdinal("OrderID")),
                                ProductID = dataReader.GetInt32(dataReader.GetOrdinal("ProductID")),
                                BuyerID = dataReader.GetInt32(dataReader.GetOrdinal("BuyerID")),
                                OrderSummaryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderHistoryID")),
                                ProductType = dataReader.GetInt32(dataReader.GetOrdinal("ProductType")),
                                PaymentMethod = dataReader.GetString(dataReader.GetOrdinal("PaymentMethod")),
                                OrderDate = dataReader.GetDateTime(dataReader.GetOrdinal("OrderDate"))
                            };
                            orderList.Add(orderItem);
                        }
                    }
                }
            }
            return orderList;
        }

        /// <summary>
        /// Gets all orders from a specific order history
        /// </summary>
        /// <param name="orderHistoryId">ID of the order history</param>
        /// <returns>List of orders in the specified order history</returns>
        public async Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId)
        {
            List<Order> orderList = new List<Order>();

            using (IDbConnection connection = _databaseProvider.CreateConnection(_connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "get_orders_from_order_history";
                    command.CommandType = CommandType.StoredProcedure;
                    AddParameter(command, "@OrderHistoryID", orderHistoryId);
                    await connection.OpenAsync();

                    using (IDataReader dataReader = await command.ExecuteReaderAsync())
                    {
                        while (await dataReader.ReadAsync())
                        {
                            Order orderItem = new Order()
                            {
                                OrderID = dataReader.GetInt32(dataReader.GetOrdinal("OrderID")),
                                ProductID = dataReader.GetInt32(dataReader.GetOrdinal("ProductID")),
                                BuyerID = dataReader.GetInt32(dataReader.GetOrdinal("BuyerID")),
                                OrderSummaryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderSummaryID")),
                                OrderHistoryID = dataReader.GetInt32(dataReader.GetOrdinal("OrderHistoryID")),
                                ProductType = dataReader.GetInt32(dataReader.GetOrdinal("ProductType")),
                                PaymentMethod = dataReader.IsDBNull(dataReader.GetOrdinal("PaymentMethod")) ? string.Empty : dataReader.GetString(dataReader.GetOrdinal("PaymentMethod")),
                                OrderDate = dataReader.IsDBNull(dataReader.GetOrdinal("OrderDate")) ? DateTime.MinValue : dataReader.GetDateTime(dataReader.GetOrdinal("OrderDate"))
                            };
                            orderList.Add(orderItem);
                        }
                    }
                }
            }

            return orderList;
        }


    }
}

