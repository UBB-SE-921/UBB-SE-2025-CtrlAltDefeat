using ArtAttack.Domain;
using ArtAttack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtAttack.ViewModel
{
    /// <summary>
    /// View model for managing tracked orders and their checkpoints
    /// </summary>
    internal class TrackedOrderViewModel : ITrackedOrderViewModel
    {
        private readonly ITrackedOrderModel trackedOrderModel;

        /// <summary>
        /// Initializes a new instance of the TrackedOrderViewModel
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        public TrackedOrderViewModel(string connectionString)
        {
            trackedOrderModel = new TrackedOrderModel(connectionString);
        }

        /// <summary>
        /// Retrieves a tracked order by its ID
        /// </summary>
        /// <param name="trackedOrderID">The ID of the tracked order to retrieve</param>
        /// <returns>The tracked order with the specified ID</returns>
        public async Task<TrackedOrder> GetTrackedOrderByIDAsync(int trackedOrderID)
        {
            return await trackedOrderModel.GetTrackedOrderByIdAsync(trackedOrderID);
        }

        /// <summary>
        /// Retrieves an order checkpoint by its ID
        /// </summary>
        /// <param name="checkpointID">The ID of the checkpoint to retrieve</param>
        /// <returns>The order checkpoint with the specified ID</returns>
        public async Task<OrderCheckpoint> GetOrderCheckpointByIDAsync(int checkpointID)
        {
            return await trackedOrderModel.GetOrderCheckpointByIdAsync(checkpointID);
        }

        /// <summary>
        /// Retrieves all tracked orders
        /// </summary>
        /// <returns>A list of all tracked orders</returns>
        public async Task<List<TrackedOrder>> GetAllTrackedOrdersAsync()
        {
            return await trackedOrderModel.GetAllTrackedOrdersAsync();
        }

        /// <summary>
        /// Retrieves all checkpoints for a specific tracked order
        /// </summary>
        /// <param name="trackedOrderID">The ID of the tracked order</param>
        /// <returns>A list of all checkpoints for the specified tracked order</returns>
        public async Task<List<OrderCheckpoint>> GetAllOrderCheckpointsAsync(int trackedOrderID)
        {
            return await trackedOrderModel.GetAllOrderCheckpointsAsync(trackedOrderID);
        }

        /// <summary>
        /// Deletes a tracked order by its ID
        /// </summary>
        /// <param name="trackedOrderID">The ID of the tracked order to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public async Task<bool> DeleteTrackedOrderAsync(int trackedOrderID)
        {
            return await trackedOrderModel.DeleteTrackedOrderAsync(trackedOrderID);
        }

        /// <summary>
        /// Deletes an order checkpoint by its ID
        /// </summary>
        /// <param name="checkpointID">The ID of the checkpoint to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public async Task<bool> DeleteOrderCheckpointAsync(int checkpointID)
        {
            return await trackedOrderModel.DeleteOrderCheckpointAsync(checkpointID);
        }

        /// <summary>
        /// Adds a new tracked order and sends a notification to the buyer
        /// </summary>
        /// <param name="trackedOrder">The tracked order to add</param>
        /// <returns>The ID of the newly added tracked order</returns>
        /// <exception cref="Exception">Thrown when there's an error adding the tracked order</exception>
        public async Task<int> AddTrackedOrderAsync(TrackedOrder trackedOrder)
        {
            try
            {
                int newTrackedOrderID = await trackedOrderModel.AddTrackedOrderAsync(trackedOrder);
                IOrderViewModel orderViewModel = new OrderViewModel(Shared.Configuration._CONNECTION_STRING_);
                try
                {
                    Order order = await orderViewModel.GetOrderByIdAsync(trackedOrder.OrderID);
                    NotificationViewModel buyerNotificationViewModel = new NotificationViewModel(order.BuyerID);
                    Notification orderShippingNotification = new OrderShippingProgressNotification(
                        order.BuyerID,
                        DateTime.Now,
                        trackedOrder.TrackedOrderID,
                        trackedOrder.CurrentStatus.ToString(),
                        trackedOrder.EstimatedDeliveryDate.ToDateTime(TimeOnly.FromDateTime(DateTime.Now))
                    );
                    await buyerNotificationViewModel.AddNotificationAsync(orderShippingNotification);
                }
                catch (Exception)
                {
                    //throw new Exception("Notification could not be sent");
                }
                return newTrackedOrderID;
            }
            catch (Exception exception)
            {
                throw new Exception("Error adding TrackedOrder\n" + exception.ToString());
            }
        }

        /// <summary>
        /// Adds a new order checkpoint and updates the tracked order status
        /// </summary>
        /// <param name="checkpoint">The checkpoint to add</param>
        /// <returns>The ID of the newly added checkpoint</returns>
        /// <exception cref="Exception">Thrown when there's an error adding the checkpoint</exception>
        public async Task<int> AddOrderCheckpointAsync(OrderCheckpoint checkpoint)
        {
            try
            {
                int newCheckpointID = await trackedOrderModel.AddOrderCheckpointAsync(checkpoint);
                TrackedOrder trackedOrder = await GetTrackedOrderByIDAsync(checkpoint.TrackedOrderID);
                await UpdateTrackedOrderAsync(trackedOrder.TrackedOrderID, trackedOrder.EstimatedDeliveryDate, checkpoint.Status);

                if (checkpoint.Status == OrderStatus.SHIPPED || checkpoint.Status == OrderStatus.OUT_FOR_DELIVERY)
                {
                    try
                    {
                        IOrderViewModel orderViewModel = new OrderViewModel(Shared.Configuration._CONNECTION_STRING_);
                        Order order = await orderViewModel.GetOrderByIdAsync(trackedOrder.OrderID);
                        NotificationViewModel buyerNotificationViewModel = new NotificationViewModel(order.BuyerID);
                        Notification shippingProgressNotification = new OrderShippingProgressNotification(
                            order.BuyerID,
                            DateTime.Now,
                            trackedOrder.TrackedOrderID,
                            trackedOrder.CurrentStatus.ToString(),
                            trackedOrder.EstimatedDeliveryDate.ToDateTime(TimeOnly.FromDateTime(DateTime.Now))
                        );
                        await buyerNotificationViewModel.AddNotificationAsync(shippingProgressNotification);
                    }
                    catch (Exception)
                    {
                        //throw new Exception("Notification could not be sent");
                    }
                }
                return newCheckpointID;
            }
            catch (Exception exception)
            {
                throw new Exception("Error adding OrderCheckpoint\n" + exception.ToString());
            }
        }

        /// <summary>
        /// Updates an existing order checkpoint and updates the tracked order status
        /// </summary>
        /// <param name="checkpointID">The ID of the checkpoint to update</param>
        /// <param name="timestamp">The new timestamp</param>
        /// <param name="location">The new location</param>
        /// <param name="description">The new description</param>
        /// <param name="status">The new order status</param>
        /// <exception cref="Exception">Thrown when there's an error updating the checkpoint</exception>
        public async Task UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string? location, string description, OrderStatus status)
        {
            try
            {
                await trackedOrderModel.UpdateOrderCheckpointAsync(checkpointID, timestamp, location, description, status);

                OrderCheckpoint updatedCheckpoint = await GetOrderCheckpointByIDAsync(checkpointID);
                TrackedOrder associatedTrackedOrder = await GetTrackedOrderByIDAsync(updatedCheckpoint.TrackedOrderID);

                await UpdateTrackedOrderAsync(associatedTrackedOrder.TrackedOrderID, associatedTrackedOrder.EstimatedDeliveryDate, updatedCheckpoint.Status);
            }
            catch (Exception exception)
            {
                throw new Exception("Error updating OrderCheckpoint\n" + exception.ToString());
            }
        }

        /// <summary>
        /// Updates an existing tracked order with new status and delivery date
        /// </summary>
        /// <param name="trackedOrderID">The ID of the tracked order to update</param>
        /// <param name="estimatedDeliveryDate">The new estimated delivery date</param>
        /// <param name="currentStatus">The new order status</param>
        /// <exception cref="Exception">Thrown when there's an error updating the tracked order</exception>
        public async Task UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, OrderStatus currentStatus)
        {
            try
            {
                await trackedOrderModel.UpdateTrackedOrderAsync(trackedOrderID, estimatedDeliveryDate, currentStatus);
                TrackedOrder updatedTrackedOrder = await GetTrackedOrderByIDAsync(trackedOrderID);

                if (updatedTrackedOrder.CurrentStatus == OrderStatus.SHIPPED || updatedTrackedOrder.CurrentStatus == OrderStatus.OUT_FOR_DELIVERY)
                {
                    try
                    {
                        IOrderViewModel orderViewModel = new OrderViewModel(Shared.Configuration._CONNECTION_STRING_);
                        Order order = await orderViewModel.GetOrderByIdAsync(updatedTrackedOrder.OrderID);
                        NotificationViewModel buyerNotificationViewModel = new NotificationViewModel(order.BuyerID);
                        Notification shippingProgressNotification = new OrderShippingProgressNotification(
                            order.BuyerID,
                            DateTime.Now,
                            updatedTrackedOrder.TrackedOrderID,
                            updatedTrackedOrder.CurrentStatus.ToString(),
                            updatedTrackedOrder.EstimatedDeliveryDate.ToDateTime(TimeOnly.FromDateTime(DateTime.Now))
                        );
                        await buyerNotificationViewModel.AddNotificationAsync(shippingProgressNotification);
                    }
                    catch (Exception)
                    {
                        //throw new Exception("Notification could not be sent");
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Error updating TrackedOrder\n" + exception.ToString());
            }
        }

        /// <summary>
        /// Reverts the tracked order to its previous checkpoint state
        /// </summary>
        /// <param name="order">The tracked order to revert</param>
        /// <exception cref="Exception">Thrown when the order cannot be reverted or an error occurs</exception>
        public async Task RevertToPreviousCheckpoint(TrackedOrder order)
        {
            int checkpointCount = await GetNumberOfCheckpoints(order);
            const int minimumCheckpointsForReversion = 1;

            if (checkpointCount <= minimumCheckpointsForReversion)
                throw new Exception("Cannot revert further");

            var currentCheckpoint = await GetLastCheckpoint(order);
            if (currentCheckpoint != null)
            {
                OrderCheckpoint currentCheckpointCast = (OrderCheckpoint)currentCheckpoint;
                bool deletionSuccessful = await DeleteOrderCheckpointAsync(currentCheckpointCast.CheckpointID);
                if (deletionSuccessful)
                {
                    OrderCheckpoint previousCheckpoint = (OrderCheckpoint)await GetLastCheckpoint(order);
                    await UpdateTrackedOrderAsync(order.TrackedOrderID, order.EstimatedDeliveryDate, previousCheckpoint.Status);
                }
                else
                    throw new Exception("Unexpected error when trying to delete the current checkpoint");
            }
            else
                throw new Exception("Unexpected error when trying to revert to the previous checkpoint");
        }

        /// <summary>
        /// Gets the most recent checkpoint for a tracked order
        /// </summary>
        /// <param name="order">The tracked order</param>
        /// <returns>The most recent checkpoint, or null if none exists</returns>
        public async Task<OrderCheckpoint?> GetLastCheckpoint(TrackedOrder order)
        {
            List<OrderCheckpoint> allCheckpoints = await GetAllOrderCheckpointsAsync(order.TrackedOrderID);
            OrderCheckpoint? lastCheckpoint = allCheckpoints.LastOrDefault();
            return lastCheckpoint;
        }

        /// <summary>
        /// Gets the total number of checkpoints for a tracked order
        /// </summary>
        /// <param name="order">The tracked order</param>
        /// <returns>The number of checkpoints</returns>
        public async Task<int> GetNumberOfCheckpoints(TrackedOrder order)
        {
            List<OrderCheckpoint> allCheckpoints = await GetAllOrderCheckpointsAsync(order.TrackedOrderID);
            return allCheckpoints.Count;
        }

        /// <summary>
        /// Method not implemented
        /// </summary>
        /// <param name="order">The tracked order</param>
        /// <returns>Not implemented</returns>
        /// <exception cref="NotImplementedException">Always thrown as method is not implemented</exception>
        public Task<bool> RevertToLastCheckpoint(TrackedOrder order)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method not implemented
        /// </summary>
        /// <param name="checkpointID">The checkpoint ID</param>
        /// <param name="timestamp">The timestamp</param>
        /// <param name="location">The location</param>
        /// <param name="description">The description</param>
        /// <param name="status">The status</param>
        /// <param name="trackedOrderID">The tracked order ID</param>
        /// <returns>Not implemented</returns>
        /// <exception cref="NotImplementedException">Always thrown as method is not implemented</exception>
        Task<bool> ITrackedOrderViewModel.UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string? location, string description, OrderStatus status, int trackedOrderID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method not implemented
        /// </summary>
        /// <param name="trackedOrderID">The tracked order ID</param>
        /// <param name="estimatedDeliveryDate">The estimated delivery date</param>
        /// <param name="deliveryAddress">The delivery address</param>
        /// <param name="currentStatus">The current status</param>
        /// <param name="orderID">The order ID</param>
        /// <returns>Not implemented</returns>
        /// <exception cref="NotImplementedException">Always thrown as method is not implemented</exception>
        Task<bool> ITrackedOrderViewModel.UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, string deliveryAddress, OrderStatus currentStatus, int orderID)
        {
            throw new NotImplementedException();
        }
    }
}