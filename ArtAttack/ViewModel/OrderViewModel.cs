using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using Microsoft.Data.SqlClient;

namespace ArtAttack.ViewModel
{
    public class OrderViewModel : IOrderViewModel
    {
        private readonly IOrderModel model;
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        public OrderViewModel(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        public OrderViewModel(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
            model = new OrderModel(connectionString, databaseProvider);
        }

        public async Task AddOrderAsync(int productId, int buyerId, int productType, string paymentMethod, int orderSummaryId, DateTime orderDate)
        {
            await model.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate);
        }

        public async Task UpdateOrderAsync(int orderId, int productType, string paymentMethod, DateTime orderDate)
        {
            await model.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate);
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            await model.DeleteOrderAsync(orderId);
        }

        public async Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId)
        {
            return await model.GetBorrowedOrderHistoryAsync(buyerId);
        }

        public async Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId)
        {
            return await model.GetNewOrUsedOrderHistoryAsync(buyerId);
        }

        public async Task<List<Order>> GetOrdersFromLastThreeMonthsAsync(int buyerId)
        {
            return await Task.Run(() => model.GetOrdersFromLastThreeMonths(buyerId));
        }

        public async Task<List<Order>> GetOrdersFromLastSixMonthsAsync(int buyerId)
        {
            return await Task.Run(() => model.GetOrdersFromLastSixMonths(buyerId));
        }

        public async Task<List<Order>> GetOrdersFrom2024Async(int buyerId)
        {
            return await Task.Run(() => model.GetOrdersFrom2024(buyerId));
        }

        public async Task<List<Order>> GetOrdersFrom2025Async(int buyerId)
        {
            return await Task.Run(() => model.GetOrdersFrom2025(buyerId));
        }

        public async Task<List<Order>> GetOrdersByNameAsync(int buyerId, string text)
        {
            return await Task.Run(() => model.GetOrdersByName(buyerId, text));
        }

        public async Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId)
        {
            return await model.GetOrdersFromOrderHistoryAsync(orderHistoryId);
        }

        public async Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId)
        {
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    string query = @"SELECT * FROM [OrderSummary] WHERE ID = @OrderSummaryId";
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@OrderSummaryId", orderSummaryId);

                    await conn.OpenAsync();
                    using (IDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new OrderSummary()
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
                                AdditionalInfo = reader.IsDBNull(reader.GetOrdinal("AdditionalInfo")) ? string.Empty : reader.GetString(reader.GetOrdinal("AdditionalInfo")),
                                ContractDetails = reader.IsDBNull(reader.GetOrdinal("ContractDetails")) ? string.Empty : reader.GetString(reader.GetOrdinal("ContractDetails"))
                            };
                        }
                    }
                }
            }
            throw new KeyNotFoundException($"OrderSummary with ID {orderSummaryId} not found");
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            var borrowedOrders = await model.GetBorrowedOrderHistoryAsync(0);
            foreach (var order in borrowedOrders)
            {
                if (order.OrderID == orderId)
                {
                    return order;
                }
            }

            var newUsedOrders = await model.GetNewOrUsedOrderHistoryAsync(0);
            foreach (var order in newUsedOrders)
            {
                if (order.OrderID == orderId)
                {
                    return order;
                }
            }

            return null;
        }

        public async Task<List<Order>> GetCombinedOrderHistoryAsync(int buyerId, string timePeriodFilter = "all")
        {
            List<Order> orders = new List<Order>();

            switch (timePeriodFilter.ToLower())
            {
                case "3months":
                    orders = model.GetOrdersFromLastThreeMonths(buyerId);
                    break;
                case "6months":
                    orders = model.GetOrdersFromLastSixMonths(buyerId);
                    break;
                case "2024":
                    orders = model.GetOrdersFrom2024(buyerId);
                    break;
                case "2025":
                    orders = model.GetOrdersFrom2025(buyerId);
                    break;
                case "all":
                default:
                    var borrowedOrders = await model.GetBorrowedOrderHistoryAsync(buyerId);
                    var newUsedOrders = await model.GetNewOrUsedOrderHistoryAsync(buyerId);
                    orders.AddRange(borrowedOrders);
                    orders.AddRange(newUsedOrders);
                    break;
            }

            return orders;
        }

        /// <summary>
        /// Retrieves order data with product information for a specified user.
        /// </summary>
        /// <param name="userId">The ID of the user whose orders to retrieve. Must be a positive integer.</param>
        /// <param name="searchText">Optional. Text to filter orders by product name. Can be null or empty.</param>
        /// <param name="timePeriod">Optional. Time period filter ("Last 3 Months", "Last 6 Months", "This Year", etc.). Can be null or empty.</param>
        /// <returns>A list of OrderDisplayInfo objects containing order details with product information.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the database command.</exception>
        /// <exception cref="ArgumentException">Thrown when userId is less than or equal to zero.</exception>
        /// <remarks>
        /// The method returns an empty list if no orders are found matching the criteria.
        /// Orders are categorized as either "new" or "borrowed" based on the product type.
        /// </remarks>
        public async Task<List<OrderDisplayInfo>> GetOrdersWithProductInfoAsync(int userId, string searchText = null, string timePeriod = null)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be positive", nameof(userId));
            }

            List<OrderDisplayInfo> orderDisplayInfos = new List<OrderDisplayInfo>();
            Dictionary<int, string> productCategoryTypes = new Dictionary<int, string>();

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

                            // Determine product type category
                            string productCategory;
                            if (productTypeName == "new" || productTypeName == "used")
                            {
                                productCategory = "new";
                                productCategoryTypes[orderSummaryId] = "new";
                            }
                            else
                            {
                                productCategory = "borrowed";
                                productCategoryTypes[orderSummaryId] = "borrowed";
                            }

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

        /// <summary>
        /// Retrieves product category types (new/borrowed) for each order summary ID for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose product categories to retrieve. Must be a positive integer.</param>
        /// <returns>A dictionary mapping order summary IDs to product category types ("new" or "borrowed").</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the database command.</exception>
        /// <exception cref="ArgumentException">Thrown when userId is less than or equal to zero.</exception>
        /// <remarks>
        /// Products are categorized as "new" if their type is "new" or "used", otherwise as "borrowed".
        /// This information is used to determine whether to show contract details for borrowed products.
        /// </remarks>
        public async Task<Dictionary<int, string>> GetProductCategoryTypesAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be positive", nameof(userId));
            }

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

                            if (productTypeName == "new" || productTypeName == "used")
                            {
                                productCategoryTypes[orderSummaryId] = "new";
                            }
                            else
                            {
                                productCategoryTypes[orderSummaryId] = "borrowed";
                            }
                        }
                    }
                }
            }

            return productCategoryTypes;
        }
    }

    /// <summary>
    /// Class representing order details with product information.
    /// </summary>
    public class OrderDisplayInfo
    {
        public int OrderID { get; set; }
        public string ProductName { get; set; }
        public string ProductTypeName { get; set; }
        public string OrderDate { get; set; }
        public string PaymentMethod { get; set; }
        public int OrderSummaryID { get; set; }
        public string ProductCategory { get; set; }
    }
}