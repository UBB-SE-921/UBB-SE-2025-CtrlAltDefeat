using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        public OrderRepository(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        public OrderRepository(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString;
            this.databaseProvider = databaseProvider;
        }

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

        public async Task<List<Order>> GetOrdersByNameAsync(int buyerId, string text)
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

        public async Task<List<Order>> GetOrdersFrom2024Async(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "get_orders_from_2024";
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

        public async Task<List<Order>> GetOrdersFrom2025Async(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "get_orders_from_2025";
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

        public async Task<List<Order>> GetOrdersFromLastSixMonthsAsync(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "get_orders_from_last_6_months";
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

        public async Task<List<Order>> GetOrdersFromLastThreeMonthsAsync(int buyerId)
        {
            List<Order> orders = new List<Order>();
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "get_orders_from_last_3_months";
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

        public async Task<List<OrderDisplayInfo>> GetOrdersWithProductInfoAsync(int userId, string searchText = null, string timePeriod = null)
        {
            List<OrderDisplayInfo> orderDisplayInfos = new List<OrderDisplayInfo>();

            using (var connection = databaseProvider.CreateConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    string query = @"SELECT 
                        o.OrderID, 
                        p.name AS ProductName, 
                        o.ProductType,  
                        p.productType AS ProductTypeName,  
                        o.OrderDate, 
                        o.PaymentMethod, 
                        o.OrderSummaryID
                    FROM [Order] o
                    JOIN [DummyProduct] p ON o.ProductType = p.ID
                    WHERE o.BuyerID = @UserId";

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        query += " AND p.name LIKE @SearchText";
                    }

                    if (timePeriod == "Last 3 Months")
                    {
                        query += " AND o.OrderDate >= DATEADD(month, -3, GETDATE())";
                    }
                    else if (timePeriod == "Last 6 Months")
                    {
                        query += " AND o.OrderDate >= DATEADD(month, -6, GETDATE())";
                    }
                    else if (timePeriod == "This Year")
                    {
                        query += " AND YEAR(o.OrderDate) = YEAR(GETDATE())";
                    }

                    command.CommandText = query;
                    command.Parameters.AddWithValue("@UserId", userId);
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        command.Parameters.AddWithValue("@SearchText", $"%{searchText}%");
                    }

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var orderId = reader.GetInt32(reader.GetOrdinal("OrderID"));
                            var productName = reader.GetString(reader.GetOrdinal("ProductName"));
                            var productType = reader.GetInt32(reader.GetOrdinal("ProductType"));
                            var productTypeName = reader.GetString(reader.GetOrdinal("ProductTypeName"));
                            var orderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate"));
                            var paymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod"));
                            var orderSummaryId = reader.GetInt32(reader.GetOrdinal("OrderSummaryID"));

                            string productCategory = (productTypeName == "new" || productTypeName == "used") ? "new" : "borrowed";

                            orderDisplayInfos.Add(new OrderDisplayInfo
                            {
                                OrderID = orderId,
                                ProductName = productName,
                                ProductTypeName = productTypeName,
                                OrderDate = orderDate.ToString("yyyy-MM-dd"),
                                PaymentMethod = paymentMethod,
                                OrderSummaryID = orderSummaryId,
                                ProductCategory = productCategory
                            });
                        }
                    }
                }
            }

            return orderDisplayInfos;
        }

        public async Task<Dictionary<int, string>> GetProductCategoryTypesAsync(int userId)
        {
            Dictionary<int, string> productCategoryTypes = new Dictionary<int, string>();

            using (var connection = databaseProvider.CreateConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    string query = @"SELECT 
                        o.OrderSummaryID,
                        p.productType
                    FROM [Order] o
                    JOIN [DummyProduct] p ON o.ProductType = p.ID
                    WHERE o.BuyerID = @UserId";

                    command.CommandText = query;
                    command.Parameters.AddWithValue("@UserId", userId);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var orderSummaryId = reader.GetInt32(reader.GetOrdinal("OrderSummaryID"));
                            var productTypeName = reader.GetString(reader.GetOrdinal("productType"));

                            productCategoryTypes[orderSummaryId] = (productTypeName == "new" || productTypeName == "used") ? "new" : "borrowed";
                        }
                    }
                }
            }

            return productCategoryTypes;
        }

        public async Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId)
        {
            using (var connection = databaseProvider.CreateConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM [OrderSummary] WHERE ID = @OrderSummaryId";
                    command.Parameters.AddWithValue("@OrderSummaryId", orderSummaryId);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new OrderSummary
                            {
                                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                                Subtotal = (float)reader.GetDouble(reader.GetOrdinal("Subtotal")),
                                WarrantyTax = (float)reader.GetDouble(reader.GetOrdinal("WarrantyTax")),
                                DeliveryFee = (float)reader.GetDouble(reader.GetOrdinal("DeliveryFee")),
                                FinalTotal = (float)reader.GetDouble(reader.GetOrdinal("FinalTotal")),
                                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                PostalCode = reader.GetString(reader.GetOrdinal("PostalCode")),
                                AdditionalInfo = reader.IsDBNull(reader.GetOrdinal("AdditionalInfo")) ? null : reader.GetString(reader.GetOrdinal("AdditionalInfo")),
                                ContractDetails = reader.IsDBNull(reader.GetOrdinal("ContractDetails")) ? null : reader.GetString(reader.GetOrdinal("ContractDetails"))
                            };
                        }
                    }
                }
            }
            throw new KeyNotFoundException($"OrderSummary with ID {orderSummaryId} not found");
        }
    }
}