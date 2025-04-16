using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.Model
{
    public interface IOrderModel
    {
        string ConnectionString { get; }

        Task AddOrderAsync(int productId, int buyerId, int productType, string paymentMethod, int orderSummaryId, DateTime orderDate);
        Task DeleteOrderAsync(int orderId);
        Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId);
        Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId);
        List<Order> GetOrdersByName(int buyerId, string text);
        List<Order> GetOrdersFrom2024(int buyerId);
        List<Order> GetOrdersFrom2025(int buyerId);
        List<Order> GetOrdersFromLastSixMonths(int buyerId);
        List<Order> GetOrdersFromLastThreeMonths(int buyerId);
        Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId);
        Task UpdateOrderAsync(int orderId, int productType, string paymentMethod, DateTime orderDate);
    }
}