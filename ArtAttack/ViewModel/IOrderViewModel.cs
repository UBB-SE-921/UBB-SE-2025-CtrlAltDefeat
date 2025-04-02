using ArtAttack.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtAttack.ViewModel
{
    public interface IOrderViewModel
    {
        Task AddOrderAsync(int productId, int buyerId, int productType, string paymentMethod, int orderSummaryId, DateTime orderDate);
        Task DeleteOrderAsync(int orderId);
        Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId);
        Task<List<Order>> GetCombinedOrderHistoryAsync(int buyerId, string timePeriodFilter = "all");
        Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId);
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetOrdersByNameAsync(int buyerId, string text);
        Task<List<Order>> GetOrdersFrom2024Async(int buyerId);
        Task<List<Order>> GetOrdersFrom2025Async(int buyerId);
        Task<List<Order>> GetOrdersFromLastSixMonthsAsync(int buyerId);
        Task<List<Order>> GetOrdersFromLastThreeMonthsAsync(int buyerId);
        Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId);
        Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId);
        Task<List<OrderDisplayInfo>> GetOrdersWithProductInfoAsync(int userId, string searchText = null, string timePeriod = null);
        Task<Dictionary<int, string>> GetProductCategoryTypesAsync(int userId);
        Task UpdateOrderAsync(int orderId, int productType, string paymentMethod, DateTime orderDate);
    }
}