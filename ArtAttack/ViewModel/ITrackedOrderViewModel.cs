using ArtAttack.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtAttack.ViewModel
{
    internal interface ITrackedOrderViewModel
    {
        Task<int> AddOrderCheckpointAsync(OrderCheckpoint checkpoint);
        Task<int> AddTrackedOrderAsync(TrackedOrder order);
        Task<bool> DeleteOrderCheckpointAsync(int checkpointID);
        Task<bool> DeleteTrackedOrderAsync(int trackOrderID);
        Task<List<OrderCheckpoint>> GetAllOrderCheckpointsAsync(int trackedOrderID);
        Task<List<TrackedOrder>> GetAllTrackedOrdersAsync();
        Task<OrderCheckpoint?> GetLastCheckpoint(TrackedOrder order);
        Task<OrderCheckpoint?> GetOrderCheckpointByIDAsync(int checkpointID);
        Task<TrackedOrder?> GetTrackedOrderByIDAsync(int trackedOrderID);
        Task<bool> RevertToLastCheckpoint(TrackedOrder order);
        Task RevertToPreviousCheckpoint(TrackedOrder? order);
        Task<bool> UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string? location, string description, OrderStatus status, int trackedOrderID);
        Task UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string location, string description, OrderStatus orderStatus);
        Task<bool> UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, string deliveryAddress, OrderStatus currentStatus, int orderID);
        Task UpdateTrackedOrderAsync(int trackedOrderID, DateOnly newEstimatedDeliveryDate, OrderStatus currentStatus);
    }
}