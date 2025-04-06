using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ArtAttack.ViewModel
{
    /// <summary>
    /// Interface defining operations for managing orders in the ArtAttack application
    /// </summary>
    public interface IOrderViewModel
    {
        Task AddOrderAsync(int productId, int buyerId, int productType, string paymentMethod, int orderSummaryId, DateTime orderDate);
        Task UpdateOrderAsync(int orderId, int productType, string paymentMethod, DateTime orderDate);
        Task DeleteOrderAsync(int orderId);
        Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId);
        Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId);
        Task<List<Order>> GetOrdersFromLastThreeMonthsAsync(int buyerId);
        Task<List<Order>> GetOrdersFromLastSixMonthsAsync(int buyerId);
        Task<List<Order>> GetOrdersFrom2024Async(int buyerId);
        Task<List<Order>> GetOrdersFrom2025Async(int buyerId);
        Task<List<Order>> GetOrdersByNameAsync(int buyerId, string searchText);
        Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId);
        Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId);
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetCombinedOrderHistoryAsync(int buyerId, string timePeriodFilter = "all");
    }

    /// <summary>
    /// Interface for retrieving order summary information
    /// </summary>
    public interface IOrderSummaryService
    {
        /// <summary>
        /// Retrieves details of a specific order summary
        /// </summary>
        /// <param name="orderSummaryId">Unique identifier of the order summary</param>
        /// <returns>Order summary details</returns>
        /// <exception cref="KeyNotFoundException">Thrown when order summary with the specified ID is not found</exception>
        Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId);
    }

    /// <summary>
    /// Provides access to order summary data using SQL Server
    /// </summary>
    public class SqlOrderSummaryService : IOrderSummaryService
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the SqlOrderSummaryService class
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        public SqlOrderSummaryService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Retrieves details of a specific order summary from the database
        /// </summary>
        /// <param name="orderSummaryId">Unique identifier of the order summary</param>
        /// <returns>Order summary details</returns>
        /// <exception cref="KeyNotFoundException">Thrown when order summary with the specified ID is not found</exception>
        public async Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId)
        {
            return await Task.Run(() =>
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string SQL_QUERY = @"SELECT * FROM [OrderSummary] WHERE ID = @OrderSummaryId";
                    SqlCommand command = new SqlCommand(SQL_QUERY, connection);

                    command.Parameters.Add("@OrderSummaryId", SqlDbType.Int).Value = orderSummaryId;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
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
                                AdditionalInfo = reader.IsDBNull("AdditionalInfo") ? string.Empty : reader.GetString("AdditionalInfo"),
                                ContractDetails = reader.IsDBNull("ContractDetails") ? string.Empty : reader.GetString("ContractDetails")
                            };
                        }
                    }
                }
                throw new KeyNotFoundException($"OrderSummary with ID {orderSummaryId} not found");
            });
        }
    }

    /// <summary>
    /// Implementation of the IOrderViewModel interface providing order management functionality
    /// </summary>
    public class OrderViewModel : IOrderViewModel
    {
        private readonly IOrderModel _orderModel;
        private readonly IOrderSummaryService _orderSummaryService;
        private const int ALL_ORDERS_DEFAULT_BUYER_ID = 0;

        // Product type constants to replace magic numbers
        public static class ProductType
        {
            public const int Borrowed = 0;
            public const int New = 1;
            public const int Used = 2;
        }

        // Time period filter constants
        public static class TimePeriodFilter
        {
            public const string ThreeMonths = "3months";
            public const string SixMonths = "6months";
            public const string Year2024 = "2024";
            public const string Year2025 = "2025";
            public const string All = "all";
        }

        /// <summary>
        /// Initializes a new instance of the OrderViewModel class with default implementations
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        public OrderViewModel(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            var databaseProvider = new SqlDatabaseProvider();
            _orderModel = new OrderModel(connectionString, databaseProvider);
            _orderSummaryService = new SqlOrderSummaryService(connectionString);
        }

        /// <summary>
        /// Initializes a new instance of the OrderViewModel class with specified dependencies (for testing)
        /// </summary>
        /// <param name="orderModel">Order model implementation</param>
        /// <param name="orderSummaryService">Order summary service implementation</param>
        public OrderViewModel(IOrderModel orderModel, IOrderSummaryService orderSummaryService)
        {
            _orderModel = orderModel ?? throw new ArgumentNullException(nameof(orderModel));
            _orderSummaryService = orderSummaryService ?? throw new ArgumentNullException(nameof(orderSummaryService));
        }

        /// <summary>
        /// Adds a new order to the system
        /// </summary>
        /// <param name="productId">Unique identifier of the product being ordered</param>
        /// <param name="buyerId">Unique identifier of the customer placing the order</param>
        /// <param name="productType">Type of product (0: Borrowed, 1: New, 2: Used)</param>
        /// <param name="paymentMethod">Method used for payment (e.g., "Credit Card", "PayPal")</param>
        /// <param name="orderSummaryId">Unique identifier of the associated order summary</param>
        /// <param name="orderDate">Date and time when the order was placed</param>
        public async Task AddOrderAsync(int productId, int buyerId, int productType, string paymentMethod, int orderSummaryId, DateTime orderDate)
        {
            await Task.Run(() => _orderModel.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate));
        }

        /// <summary>
        /// Updates an existing order's information
        /// </summary>
        /// <param name="orderId">Unique identifier of the order to update</param>
        /// <param name="productType">Updated product type (0: Borrowed, 1: New, 2: Used)</param>
        /// <param name="paymentMethod">Updated payment method</param>
        /// <param name="orderDate">Updated order date</param>
        public async Task UpdateOrderAsync(int orderId, int productType, string paymentMethod, DateTime orderDate)
        {
            await Task.Run(() => _orderModel.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate));
        }

        /// <summary>
        /// Deletes an order from the system
        /// </summary>
        /// <param name="orderId">Unique identifier of the order to delete</param>
        public async Task DeleteOrderAsync(int orderId)
        {
            await Task.Run(() => _orderModel.DeleteOrderAsync(orderId));
        }

        /// <summary>
        /// Retrieves the history of borrowed items for a specific buyer
        /// </summary>
        /// <param name="buyerId">Unique identifier of the buyer</param>
        /// <returns>List of borrowed orders</returns>
        public async Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId)
        {
            return await Task.Run(() => _orderModel.GetBorrowedOrderHistoryAsync(buyerId));
        }

        /// <summary>
        /// Retrieves the history of new or used items for a specific buyer
        /// </summary>
        /// <param name="buyerId">Unique identifier of the buyer</param>
        /// <returns>List of new or used orders</returns>
        public async Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId)
        {
            return await Task.Run(() => _orderModel.GetNewOrUsedOrderHistoryAsync(buyerId));
        }

        /// <summary>
        /// Retrieves orders from the last three months for a specific buyer
        /// </summary>
        /// <param name="buyerId">Unique identifier of the buyer</param>
        /// <returns>List of orders from the last three months</returns>
        public async Task<List<Order>> GetOrdersFromLastThreeMonthsAsync(int buyerId)
        {
            return await Task.Run(() => _orderModel.GetOrdersFromLastThreeMonths(buyerId));
        }

        /// <summary>
        /// Retrieves orders from the last six months for a specific buyer
        /// </summary>
        /// <param name="buyerId">Unique identifier of the buyer</param>
        /// <returns>List of orders from the last six months</returns>
        public async Task<List<Order>> GetOrdersFromLastSixMonthsAsync(int buyerId)
        {
            return await Task.Run(() => _orderModel.GetOrdersFromLastSixMonths(buyerId));
        }

        /// <summary>
        /// Retrieves orders from 2024 for a specific buyer
        /// </summary>
        /// <param name="buyerId">Unique identifier of the buyer</param>
        /// <returns>List of orders from 2024</returns>
        public async Task<List<Order>> GetOrdersFrom2024Async(int buyerId)
        {
            return await Task.Run(() => _orderModel.GetOrdersFrom2024(buyerId));
        }

        /// <summary>
        /// Retrieves orders from 2025 for a specific buyer
        /// </summary>
        /// <param name="buyerId">Unique identifier of the buyer</param>
        /// <returns>List of orders from 2025</returns>
        public async Task<List<Order>> GetOrdersFrom2025Async(int buyerId)
        {
            return await Task.Run(() => _orderModel.GetOrdersFrom2025(buyerId));
        }

        /// <summary>
        /// Searches for orders by name for a specific buyer
        /// </summary>
        /// <param name="buyerId">Unique identifier of the buyer</param>
        /// <param name="searchText">Text to search for in order names</param>
        /// <returns>List of matching orders</returns>
        public async Task<List<Order>> GetOrdersByNameAsync(int buyerId, string searchText)
        {
            return await Task.Run(() => _orderModel.GetOrdersByName(buyerId, searchText));
        }

        /// <summary>
        /// Retrieves all orders associated with a specific order history
        /// </summary>
        /// <param name="orderHistoryId">Unique identifier of the order history</param>
        /// <returns>List of orders in the specified order history</returns>
        public async Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId)
        {
            return await Task.Run(() => _orderModel.GetOrdersFromOrderHistoryAsync(orderHistoryId));
        }

        /// <summary>
        /// Retrieves details of a specific order summary
        /// </summary>
        /// <param name="orderSummaryId">Unique identifier of the order summary</param>
        /// <returns>Order summary details</returns>
        /// <exception cref="KeyNotFoundException">Thrown when order summary with the specified ID is not found</exception>
        public async Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId)
        {
            return await _orderSummaryService.GetOrderSummaryAsync(orderSummaryId);
        }

        /// <summary>
        /// Retrieves a specific order by its identifier
        /// </summary>
        /// <param name="orderId">Unique identifier of the order</param>
        /// <returns>Order details if found, null otherwise</returns>
        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await Task.Run(async () =>
            {
                var borrowedOrders = await _orderModel.GetBorrowedOrderHistoryAsync(ALL_ORDERS_DEFAULT_BUYER_ID);

                foreach (var order in borrowedOrders)
                {
                    if (order.OrderID == orderId)
                        return order;
                }

                var newOrUsedOrders = await _orderModel.GetNewOrUsedOrderHistoryAsync(ALL_ORDERS_DEFAULT_BUYER_ID);

                foreach (var order in newOrUsedOrders)
                {
                    if (order.OrderID == orderId)
                        return order;
                }

                return null;
            });
        }

        /// <summary>
        /// Retrieves combined order history for a specific buyer with optional time period filtering
        /// </summary>
        /// <param name="buyerId">Unique identifier of the buyer</param>
        /// <param name="timePeriodFilter">Filter for time period (e.g., "3months", "6months", "2024", "2025", "all")</param>
        /// <returns>Combined list of orders matching the specified criteria</returns>
        public async Task<List<Order>> GetCombinedOrderHistoryAsync(int buyerId, string timePeriodFilter = TimePeriodFilter.All)
        {
            return await Task.Run(async () =>
            {
                List<Order> combinedOrders = new List<Order>();

                switch (timePeriodFilter.ToLower())
                {
                    case TimePeriodFilter.ThreeMonths:
                        combinedOrders = _orderModel.GetOrdersFromLastThreeMonths(buyerId);
                        break;
                    case TimePeriodFilter.SixMonths:
                        combinedOrders = _orderModel.GetOrdersFromLastSixMonths(buyerId);
                        break;
                    case TimePeriodFilter.Year2024:
                        combinedOrders = _orderModel.GetOrdersFrom2024(buyerId);
                        break;
                    case TimePeriodFilter.Year2025:
                        combinedOrders = _orderModel.GetOrdersFrom2025(buyerId);
                        break;
                    case TimePeriodFilter.All:
                    default:
                        var borrowedOrders = await _orderModel.GetBorrowedOrderHistoryAsync(buyerId);
                        var newOrUsedOrders = await _orderModel.GetNewOrUsedOrderHistoryAsync(buyerId);

                        combinedOrders.AddRange(borrowedOrders);
                        combinedOrders.AddRange(newOrUsedOrders);
                        break;
                }

                return combinedOrders;
            });
        }
    }
}
