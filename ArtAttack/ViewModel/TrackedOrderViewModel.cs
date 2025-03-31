using ArtAttack.Domain;
using ArtAttack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtAttack.ViewModel
{
    internal class TrackedOrderViewModel : ITrackedOrderViewModel
    {
        private readonly TrackedOrderModel model;

        public TrackedOrderViewModel(string connectionString)
        {
            model = new TrackedOrderModel(connectionString);
        }

        public async Task<TrackedOrder> GetTrackedOrderByIDAsync(int trackedOrderID)
        {
            return await model.GetTrackedOrderByIdAsync(trackedOrderID);
        }

        public async Task<OrderCheckpoint> GetOrderCheckpointByIDAsync(int checkpointID)
        {
            return await model.GetOrderCheckpointByIdAsync(checkpointID);
        }

        public async Task<List<TrackedOrder>> GetAllTrackedOrdersAsync()
        {
            return await model.GetAllTrackedOrdersAsync();
        }

        public async Task<List<OrderCheckpoint>> GetAllOrderCheckpointsAsync(int trackedOrderID)
        {
            return await model.GetAllOrderCheckpointsAsync(trackedOrderID);
        }

        public async Task<bool> DeleteTrackedOrderAsync(int trackOrderID)
        {
            return await model.DeleteTrackedOrderAsync(trackOrderID);
        }

        public async Task<bool> DeleteOrderCheckpointAsync(int checkpointID)
        {
            return await model.DeleteOrderCheckpointAsync(checkpointID);
        }

        public async Task<int> AddTrackedOrderAsync(TrackedOrder trackedOrder)
        {
            try
            {
                int returnedID = await model.AddTrackedOrderAsync(trackedOrder);
                OrderViewModel orderViewModel = new OrderViewModel(Shared.Configuration._CONNECTION_STRING_);
                try
                {
                    Order order = await orderViewModel.GetOrderByIdAsync(trackedOrder.OrderID);
                    NotificationViewModel buyerNotificationViewModel = new NotificationViewModel(order.BuyerID);
                    Notification placedOrderNotification = new OrderShippingProgressNotification(order.BuyerID, DateTime.Now, trackedOrder.TrackedOrderID, trackedOrder.CurrentStatus.ToString(), trackedOrder.EstimatedDeliveryDate.ToDateTime(TimeOnly.FromDateTime(DateTime.Now)));
                    await buyerNotificationViewModel.AddNotificationAsync(placedOrderNotification);
                }
                catch (Exception)
                {
                    //throw new Exception("Notification could not be sent");
                }
                return returnedID;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding TrackedOrder\n" + ex.ToString());
            }

        }

        public async Task<int> AddOrderCheckpointAsync(OrderCheckpoint checkpoint)
        {
            try
            {
                int returnedID = await model.AddOrderCheckpointAsync(checkpoint);
                TrackedOrder trackedOrder = await GetTrackedOrderByIDAsync(checkpoint.TrackedOrderID);
                await UpdateTrackedOrderAsync(trackedOrder.TrackedOrderID, trackedOrder.EstimatedDeliveryDate, checkpoint.Status);
                if (checkpoint.Status == OrderStatus.SHIPPED || checkpoint.Status == OrderStatus.OUT_FOR_DELIVERY)
                {
                    try
                    {
                        OrderViewModel orderViewModel = new OrderViewModel(Shared.Configuration._CONNECTION_STRING_);
                        Order order = await orderViewModel.GetOrderByIdAsync(trackedOrder.OrderID);
                        NotificationViewModel buyerNotificationViewModel = new NotificationViewModel(order.BuyerID);
                        Notification placedOrderNotification = new OrderShippingProgressNotification(order.BuyerID, DateTime.Now, trackedOrder.TrackedOrderID, trackedOrder.CurrentStatus.ToString(), trackedOrder.EstimatedDeliveryDate.ToDateTime(TimeOnly.FromDateTime(DateTime.Now)));
                        await buyerNotificationViewModel.AddNotificationAsync(placedOrderNotification);
                    }
                    catch (Exception)
                    {
                        //throw new Exception("Notification could not be sent");
                    }
                }
                return returnedID;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding OrderCheckpoint\n" + ex.ToString());
            }
        }

        public async Task UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string? location, string description, OrderStatus status)
        {
            try
            {
                await model.UpdateOrderCheckpointAsync(checkpointID, timestamp, location, description, status);

                OrderCheckpoint checkpoint = await GetOrderCheckpointByIDAsync(checkpointID);
                TrackedOrder trackedOrder = await GetTrackedOrderByIDAsync(checkpoint.TrackedOrderID);

                await UpdateTrackedOrderAsync(trackedOrder.TrackedOrderID, trackedOrder.EstimatedDeliveryDate, checkpoint.Status);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating OrderCheckpoint\n" + ex.ToString());
            }

        }

        public async Task UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, OrderStatus currentStatus)
        {
            try
            {
                await model.UpdateTrackedOrderAsync(trackedOrderID, estimatedDeliveryDate, currentStatus);
                TrackedOrder trackedOrder = await GetTrackedOrderByIDAsync(trackedOrderID);
                if (trackedOrder.CurrentStatus == OrderStatus.SHIPPED || trackedOrder.CurrentStatus == OrderStatus.OUT_FOR_DELIVERY)
                {
                    try
                    {
                        OrderViewModel orderViewModel = new OrderViewModel(Shared.Configuration._CONNECTION_STRING_);
                        Order order = await orderViewModel.GetOrderByIdAsync(trackedOrder.OrderID);
                        NotificationViewModel buyerNotificationViewModel = new NotificationViewModel(order.BuyerID);
                        Notification placedOrderNotification = new OrderShippingProgressNotification(order.BuyerID, DateTime.Now, trackedOrder.TrackedOrderID, trackedOrder.CurrentStatus.ToString(), trackedOrder.EstimatedDeliveryDate.ToDateTime(TimeOnly.FromDateTime(DateTime.Now)));
                        await buyerNotificationViewModel.AddNotificationAsync(placedOrderNotification);
                    }
                    catch (Exception)
                    {
                        //throw new Exception("Notification could not be sent");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating TrackedOrder\n" + ex.ToString());
            }
        }

        public async Task RevertToPreviousCheckpoint(TrackedOrder order)
        {
            int initialNrOfCheckpoints = await GetNumberOfCheckpoints(order);
            if (initialNrOfCheckpoints <= 1)
                throw new Exception("Cannot revert further");

            var lastCheckpoint = await GetLastCheckpoint(order);
            if (lastCheckpoint != null)
            {
                OrderCheckpoint lastCheckpointCast = (OrderCheckpoint)lastCheckpoint;
                bool deleteSuccessful = await DeleteOrderCheckpointAsync(lastCheckpointCast.CheckpointID);
                if (deleteSuccessful)
                {
                    OrderCheckpoint newLastCheckpoint = (OrderCheckpoint)await GetLastCheckpoint(order);
                    await UpdateTrackedOrderAsync(order.TrackedOrderID, order.EstimatedDeliveryDate, newLastCheckpoint.Status);

                }
                else
                    throw new Exception("Unexpected error when trying to delete the current checkpoint");
            }
            else
                throw new Exception("Unexpected error when trying to revert to the previous checkpoint");
        }

        public async Task<OrderCheckpoint?> GetLastCheckpoint(TrackedOrder order)
        {
            List<OrderCheckpoint> allCheckpoints = await GetAllOrderCheckpointsAsync(order.TrackedOrderID);
            OrderCheckpoint? lastCheckpoint = allCheckpoints.LastOrDefault();
            return lastCheckpoint;
        }

        public async Task<int> GetNumberOfCheckpoints(TrackedOrder order)
        {
            List<OrderCheckpoint> allCheckpoints = await GetAllOrderCheckpointsAsync(order.TrackedOrderID);
            return allCheckpoints.Count;
        }

        public Task<bool> RevertToLastCheckpoint(TrackedOrder order)
        {
            throw new NotImplementedException();
        }

        Task<bool> ITrackedOrderViewModel.UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string? location, string description, OrderStatus status, int trackedOrderID)
        {
            throw new NotImplementedException();
        }

        Task<bool> ITrackedOrderViewModel.UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, string deliveryAddress, OrderStatus currentStatus, int orderID)
        {
            throw new NotImplementedException();
        }
    }
}