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
        Task<List<Order>> GetOrdersByNameAsync(int buyerId, string text);
        Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId);
        Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId);
        Task<Order> GetOrderByIdAsync(int orderId);
    }

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
    }
}