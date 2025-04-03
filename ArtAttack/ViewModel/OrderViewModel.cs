using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ArtAttack.ViewModel
{

    public class OrderViewModel : IOrderViewModel
    {
        private readonly OrderModel _model;

        public OrderViewModel(string connectionString)
        {
            _model = new OrderModel(connectionString);
        }

        public async Task AddOrderAsync(int productId, int buyerId, int productType, string paymentMethod, int orderSummaryId, DateTime orderDate)
        {
            await Task.Run(() => _model.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate));
        }

        public async Task UpdateOrderAsync(int orderId, int productType, string paymentMethod, DateTime orderDate)
        {
            await Task.Run(() => _model.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate));
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            await Task.Run(() => _model.DeleteOrderAsync(orderId));
        }

        public async Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId)
        {
            return await Task.Run(() => _model.GetBorrowedOrderHistoryAsync(buyerId));
        }

        public async Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId)
        {
            return await Task.Run(() => _model.GetNewOrUsedOrderHistoryAsync(buyerId));
        }

        public async Task<List<Order>> GetOrdersFromLastThreeMonthsAsync(int buyerId)
        {
            return await Task.Run(() => _model.GetOrdersFromLastThreeMonths(buyerId));
        }

        public async Task<List<Order>> GetOrdersFromLastSixMonthsAsync(int buyerId)
        {
            return await Task.Run(() => _model.GetOrdersFromLastSixMonths(buyerId));
        }

        public async Task<List<Order>> GetOrdersFrom2024Async(int buyerId)
        {
            return await Task.Run(() => _model.GetOrdersFrom2024(buyerId));
        }

        public async Task<List<Order>> GetOrdersFrom2025Async(int buyerId)
        {
            return await Task.Run(() => _model.GetOrdersFrom2025(buyerId));
        }

        public async Task<List<Order>> GetOrdersByNameAsync(int buyerId, string text)
        {
            return await Task.Run(() => _model.GetOrdersByName(buyerId, text));
        }

        public async Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId)
        {
            return await Task.Run(() => _model.GetOrdersFromOrderHistoryAsync(orderHistoryId));
        }

        public async Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId)
        {
            return await Task.Run(() =>
            {
                using (SqlConnection conn = new SqlConnection(_model.ConnectionString))
                {
                    string query = @"SELECT * FROM [OrderSummary] WHERE ID = @OrderSummaryId";
                    SqlCommand cmd = new SqlCommand(query, conn);


                    cmd.Parameters.Add("@OrderSummaryId", SqlDbType.Int).Value = orderSummaryId;

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new OrderSummary()
                            {
                                ID = reader.GetInt32("ID"),
                                Subtotal = (float)reader.GetDouble("Subtotal"),
                                WarrantyTax = (float)reader.GetDouble("WarrantyTax"),
                                DeliveryFee = (float)reader.GetDouble("DeliveryFee"),
                                FinalTotal = (float)reader.GetDouble("FinalTotal"),
                                FullName = reader.GetString("FullName"),
                                Email = reader.GetString("Email"),
                                PhoneNumber = reader.GetString("PhoneNumber"),
                                Address = reader.GetString("Address"),
                                PostalCode = reader.GetString("PostalCode"),
                                AdditionalInfo = reader.IsDBNull("AdditionalInfo") ? "" : reader.GetString("AdditionalInfo"),
                                ContractDetails = reader.IsDBNull("ContractDetails") ? "" : reader.GetString("ContractDetails")
                            };
                        }
                    }
                }
                throw new KeyNotFoundException($"OrderSummary with ID {orderSummaryId} not found");
            });
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await Task.Run(async () =>
            {
                var borrowedOrders = await _model.GetBorrowedOrderHistoryAsync(0);

                foreach (var order in borrowedOrders)
                {
                    if (order.OrderID == orderId)
                        return order;
                }

                var newUsedOrders = await _model.GetNewOrUsedOrderHistoryAsync(0);

                foreach (var order in newUsedOrders)
                {
                    if (order.OrderID == orderId)
                        return order;
                }

                return null;
            });
        }

        public async Task<List<Order>> GetCombinedOrderHistoryAsync(int buyerId, string timePeriodFilter = "all")
        {
            return await Task.Run(async () =>
            {
                List<Order> orders = new List<Order>();

                switch (timePeriodFilter.ToLower())
                {
                    case "3months":
                        orders = _model.GetOrdersFromLastThreeMonths(buyerId);
                        break;
                    case "6months":
                        orders = _model.GetOrdersFromLastSixMonths(buyerId);
                        break;
                    case "2024":
                        orders = _model.GetOrdersFrom2024(buyerId);
                        break;
                    case "2025":
                        orders = _model.GetOrdersFrom2025(buyerId);
                        break;
                    case "all":
                    default:
                        var borrowedOrders = await _model.GetBorrowedOrderHistoryAsync(buyerId);
                        var newUsedOrders = await _model.GetNewOrUsedOrderHistoryAsync(buyerId);

                        foreach (var order in borrowedOrders)
                        {
                            orders.Add(order);
                        }

                        foreach (var order in newUsedOrders)
                        {
                            orders.Add(order);
                        }
                        break;
                }

                return orders;
            });
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
            return await Task.Run(async () =>
            {
                List<OrderDisplayInfo> orderDisplayInfos = new List<OrderDisplayInfo>();
                Dictionary<int, string> productCategoryTypes = new Dictionary<int, string>();

                using (var SQLconnection = new SqlConnection(_model.ConnectionString))
                {
                    await SQLconnection.OpenAsync();

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
                        query += " AND p.name LIKE @SearchText";

                    if (timePeriod == "Last 3 Months")
                        query += " AND o.OrderDate >= DATEADD(month, -3, GETDATE())";
                    else if (timePeriod == "Last 6 Months")
                        query += " AND o.OrderDate >= DATEADD(month, -6, GETDATE())";
                    else if (timePeriod == "This Year")
                        query += " AND YEAR(o.OrderDate) = YEAR(GETDATE())";

                    using (var SQLcommand = new SqlCommand(query, SQLconnection))
                    {
                        SQLcommand.Parameters.AddWithValue("@UserId", userId);
                        if (!string.IsNullOrEmpty(searchText))
                            SQLcommand.Parameters.AddWithValue("@SearchText", $"%{searchText}%");

                        using (var sqlDataReader = await SQLcommand.ExecuteReaderAsync())
                        {
                            while (await sqlDataReader.ReadAsync())
                            {
                                var orderId = sqlDataReader.GetInt32(0);
                                var productName = sqlDataReader.GetString(1);
                                var productType = sqlDataReader.GetInt32(2);
                                var productTypeName = sqlDataReader.GetString(3);
                                var orderDate = sqlDataReader.GetDateTime(4);
                                var paymentMethod = sqlDataReader.GetString(5);
                                var orderSummaryId = sqlDataReader.GetInt32(6);

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
            });
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
            return await Task.Run(async () =>
            {
                Dictionary<int, string> productCategoryTypes = new Dictionary<int, string>();

                using (var SQLconnection = new SqlConnection(_model.ConnectionString))
                {
                    await SQLconnection.OpenAsync();

                    string query = @"SELECT 
                        o.OrderSummaryID,
                        p.productType
                    FROM [Order] o
                    JOIN [DummyProduct] p ON o.ProductType = p.ID
                    WHERE o.BuyerID = @UserId";

                    using (var SQLcommand = new SqlCommand(query, SQLconnection))
                    {
                        SQLcommand.Parameters.AddWithValue("@UserId", userId);

                        using (var sqlDataReader = await SQLcommand.ExecuteReaderAsync())
                        {
                            while (await sqlDataReader.ReadAsync())
                            {
                                var orderSummaryId = sqlDataReader.GetInt32(0);
                                var productTypeName = sqlDataReader.GetString(1);

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
            });
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