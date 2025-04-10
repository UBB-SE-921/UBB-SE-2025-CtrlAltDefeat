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
    /// <summary>
    /// Represents the view model for orders and facilitates order management operations.
    /// </summary>
    public class OrderViewModel : IOrderViewModel
    {
        private readonly IOrderModel model;
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderViewModel"/> class using the specified connection string and default SQL database provider.
        /// </summary>
        /// <param name="connectionString">The connection string used for database operations.</param>
        public OrderViewModel(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderViewModel"/> class using the specified connection string and database provider.
        /// </summary>
        /// <param name="connectionString">The connection string used for database operations.</param>
        /// <param name="databaseProvider">The database provider used for creating database connections.</param>
        public OrderViewModel(string connectionString, IDatabaseProvider databaseProvider)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            if (databaseProvider == null)
            {
                throw new ArgumentNullException(nameof(databaseProvider));
            }

            this.connectionString = connectionString;
            this.databaseProvider = databaseProvider;
            model = new OrderModel(connectionString);
        }

        /// <summary>
        /// Asynchronously adds a new order with the specified parameters.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <param name="productType">The type of the product.</param>
        /// <param name="paymentMethod">The payment method used for the order.</param>
        /// <param name="orderSummaryId">The unique identifier of the order summary.</param>
        /// <param name="orderDate">The date when the order was placed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddOrderAsync(int productId, int buyerId, int productType, string paymentMethod, int orderSummaryId, DateTime orderDate)
        {
            await model.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate);
        }

        /// <summary>
        /// Asynchronously updates the specified order with new values.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to update.</param>
        /// <param name="productType">The new product type.</param>
        /// <param name="paymentMethod">The new payment method.</param>
        /// <param name="orderDate">The new order date.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateOrderAsync(int orderId, int productType, string paymentMethod, DateTime orderDate)
        {
            await model.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate);
        }

        /// <summary>
        /// Asynchronously deletes the order specified by the order ID.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteOrderAsync(int orderId)
        {
            await model.DeleteOrderAsync(orderId);
        }

        /// <summary>
        /// Asynchronously retrieves the borrowed order history for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>A task that returns a list of borrowed orders.</returns>
        public async Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId)
        {
            return await model.GetBorrowedOrderHistoryAsync(buyerId);
        }

        /// <summary>
        /// Asynchronously retrieves the order history for new or used products for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>A task that returns a list of orders.</returns>
        public async Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId)
        {
            return await model.GetNewOrUsedOrderHistoryAsync(buyerId);
        }

        /// <summary>
        /// Asynchronously retrieves orders from the last three months for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>A task that returns a list of orders.</returns>
        public async Task<List<Order>> GetOrdersFromLastThreeMonthsAsync(int buyerId)
        {
            return await Task.Run(() => model.GetOrdersFromLastThreeMonths(buyerId));
        }

        /// <summary>
        /// Asynchronously retrieves orders from the last six months for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>A task that returns a list of orders.</returns>
        public async Task<List<Order>> GetOrdersFromLastSixMonthsAsync(int buyerId)
        {
            return await Task.Run(() => model.GetOrdersFromLastSixMonths(buyerId));
        }

        /// <summary>
        /// Asynchronously retrieves orders from the year 2024 for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>A task that returns a list of orders.</returns>
        public async Task<List<Order>> GetOrdersFrom2024Async(int buyerId)
        {
            return await Task.Run(() => model.GetOrdersFrom2024(buyerId));
        }

        /// <summary>
        /// Asynchronously retrieves orders from the year 2025 for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>A task that returns a list of orders.</returns>
        public async Task<List<Order>> GetOrdersFrom2025Async(int buyerId)
        {
            return await Task.Run(() => model.GetOrdersFrom2025(buyerId));
        }

        /// <summary>
        /// Asynchronously retrieves orders for the specified buyer based on a text search.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <param name="text">The text to search for within orders.</param>
        /// <returns>A task that returns a list of orders.</returns>
        public async Task<List<Order>> GetOrdersByNameAsync(int buyerId, string text)
        {
            return await Task.Run(() => model.GetOrdersByName(buyerId, text));
        }

        /// <summary>
        /// Asynchronously retrieves the orders associated with the specified order history ID.
        /// </summary>
        /// <param name="orderHistoryId">The unique identifier of the order history.</param>
        /// <returns>A task that returns a list of orders.</returns>
        public async Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId)
        {
            return await model.GetOrdersFromOrderHistoryAsync(orderHistoryId);
        }

        /// <summary>
        /// Asynchronously retrieves the order summary for the specified order summary ID.
        /// </summary>
        /// <param name="orderSummaryId">The unique identifier of the order summary.</param>
        /// <returns>A task that returns the <see cref="OrderSummary"/> object.</returns>
        public async Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    string query = @"SELECT * FROM [OrderSummary] WHERE ID = @OrderSummaryId";
                    databaseCommand.CommandText = query;
                    databaseCommand.Parameters.AddWithValue("@OrderSummaryId", orderSummaryId);

                    await databaseConnection.OpenAsync();
                    using (IDataReader reader = await databaseCommand.ExecuteReaderAsync())
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

        /// <summary>
        /// Asynchronously retrieves an order by its unique identifier.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order.</param>
        /// <returns>
        /// A task that returns the <see cref="Order"/> object if found; otherwise, <c>null</c>.
        /// </returns>
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

        /// <summary>
        /// Asynchronously retrieves a combined order history for the specified buyer using an optional time period filter.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <param name="timePeriodFilter">Optional filter specifying the time period ("3months", "6months", "2024", "2025", or "all").</param>
        /// <returns>A task that returns a list of orders.</returns>
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
        /// Asynchronously retrieves order details along with product information for a specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose orders to retrieve. Must be a positive integer.</param>
        /// <param name="searchText">Optional. Text to filter orders by product name. Can be null or empty.</param>
        /// <param name="timePeriod">Optional. Time period filter ("Last 3 Months", "Last 6 Months", "This Year", etc.). Can be null or empty.</param>
        /// <returns>
        /// A task that returns a list of <see cref="OrderDisplayInfo"/> objects containing order details and product information.
        /// </returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the database command.</exception>
        /// <exception cref="ArgumentException">Thrown when userId is less than or equal to zero.</exception>
        /// <remarks>
        /// The method returns an empty list if no orders match the specified criteria.
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
        /// Asynchronously retrieves product category types (new/borrowed) for each order summary ID for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose product categories to retrieve. Must be a positive integer.</param>
        /// <returns>
        /// A task that returns a dictionary mapping order summary IDs to product category types ("new" or "borrowed").
        /// </returns>
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
    /// Represents order details along with product information.
    /// </summary>
    public class OrderDisplayInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier of the order.
        /// </summary>
        public int OrderID { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the product type name.
        /// </summary>
        public string ProductTypeName { get; set; }

        /// <summary>
        /// Gets or sets the order date as a formatted string.
        /// </summary>
        public string OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the payment method used for the order.
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the order summary.
        /// </summary>
        public int OrderSummaryID { get; set; }

        /// <summary>
        /// Gets or sets the product category (either "new" or "borrowed").
        /// </summary>
        public string ProductCategory { get; set; }
    }
}
