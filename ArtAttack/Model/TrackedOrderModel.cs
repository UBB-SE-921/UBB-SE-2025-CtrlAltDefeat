using ArtAttack.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ArtAttack.Model
{
    /// <summary>
    /// Provides functionality for tracking orders and their checkpoints in the system
    /// </summary>
    class TrackedOrderModel : ITrackedOrderModel
    {
        private readonly string _connectionString;

        // SQL query constants
        private const string SELECT_ALL_ORDER_CHECKPOINTS = "SELECT * FROM OrderCheckpoints WHERE TrackedOrderID = @trackedOrderID";
        private const string SELECT_ALL_TRACKED_ORDERS = "SELECT * FROM TrackedOrders";
        private const string SELECT_ORDER_CHECKPOINT_BY_ID = "SELECT * FROM OrderCheckpoints WHERE CheckpointID = @checkpointID";
        private const string SELECT_TRACKED_ORDER_BY_ID = "SELECT * FROM TrackedOrders WHERE TrackedOrderID = @trackedOrderID";

        // Stored procedure names
        private const string SP_INSERT_ORDER_CHECKPOINT = "uspInsertOrderCheckpoint";
        private const string SP_INSERT_TRACKED_ORDER = "uspInsertTrackedOrder";
        private const string SP_DELETE_ORDER_CHECKPOINT = "uspDeleteOrderCheckpoint";
        private const string SP_DELETE_TRACKED_ORDER = "uspDeleteTrackedOrder";
        private const string SP_UPDATE_ORDER_CHECKPOINT = "uspUpdateOrderCheckpoint";
        private const string SP_UPDATE_TRACKED_ORDER = "uspUpdateTrackedOrder";

        // Error codes
        private const int ERROR_CODE_NEGATIVE = 0;

        // Database column names
        private static class DbColumns
        {
            public const string CheckpointID = "CheckpointID";
            public const string Timestamp = "Timestamp";
            public const string Location = "Location";
            public const string Description = "Description";
            public const string CheckpointStatus = "CheckpointStatus";
            public const string TrackedOrderID = "TrackedOrderID";
            public const string OrderID = "OrderID";
            public const string OrderStatus = "OrderStatus";
            public const string EstimatedDeliveryDate = "EstimatedDeliveryDate";
            public const string DeliveryAddress = "DeliveryAddress";
        }

        /// <summary>
        /// Initializes a new instance of the TrackedOrderModel class
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <exception cref="ArgumentNullException">Thrown when connection string is null</exception>
        public TrackedOrderModel(string connectionString)
        {
            this._connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Adds a new order checkpoint to the system
        /// </summary>
        /// <param name="checkpoint">The checkpoint data to add</param>
        /// <returns>The ID of the newly created checkpoint</returns>
        /// <exception cref="Exception">Thrown when the checkpoint cannot be added</exception>
        public async Task<int> AddOrderCheckpointAsync(OrderCheckpoint checkpoint)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(SP_INSERT_ORDER_CHECKPOINT, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@timestamp", checkpoint.Timestamp);
                    command.Parameters.AddWithValue("@location", checkpoint.Location ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@description", checkpoint.Description);
                    command.Parameters.AddWithValue("@checkpointStatus", checkpoint.Status.ToString());
                    command.Parameters.AddWithValue("@trackedOrderID", checkpoint.TrackedOrderID);

                    SqlParameter outputParam = new SqlParameter("@newCheckpointID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputParam);

                    await command.ExecuteNonQueryAsync();

                    int newCheckpointId = (int)command.Parameters["@newCheckpointID"].Value;
                    if (newCheckpointId <= ERROR_CODE_NEGATIVE)
                        throw new Exception("Unexpected error when trying to add the OrderCheckpoint");
                    return newCheckpointId;
                }
            }
        }

        /// <summary>
        /// Adds a new tracked order to the system
        /// </summary>
        /// <param name="order">The tracked order data to add</param>
        /// <returns>The ID of the newly created tracked order</returns>
        /// <exception cref="Exception">Thrown when the tracked order cannot be added</exception>
        public async Task<int> AddTrackedOrderAsync(TrackedOrder order)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(SP_INSERT_TRACKED_ORDER, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@estimatedDeliveryDate", order.EstimatedDeliveryDate);
                    command.Parameters.AddWithValue("@deliveryAddress", order.DeliveryAddress);
                    command.Parameters.AddWithValue("@orderStatus", order.CurrentStatus.ToString());
                    command.Parameters.AddWithValue("@orderID", order.OrderID);

                    SqlParameter outputParam = new SqlParameter("@newTrackedOrderID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputParam);

                    await command.ExecuteNonQueryAsync();

                    int newTrackedOrderId = (int)command.Parameters["@newTrackedOrderID"].Value;
                    if (newTrackedOrderId <= ERROR_CODE_NEGATIVE)
                        throw new Exception("Unexpected error when trying to add the TrackedOrder");
                    return newTrackedOrderId;
                }
            }
        }

        /// <summary>
        /// Deletes an order checkpoint from the system
        /// </summary>
        /// <param name="checkpointID">ID of the checkpoint to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public async Task<bool> DeleteOrderCheckpointAsync(int checkpointID)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(SP_DELETE_ORDER_CHECKPOINT, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@checkpointID", checkpointID);
                    return await command.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        /// <summary>
        /// Deletes a tracked order from the system
        /// </summary>
        /// <param name="trackOrderID">ID of the tracked order to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public async Task<bool> DeleteTrackedOrderAsync(int trackOrderID)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(SP_DELETE_TRACKED_ORDER, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@trackOrderID", trackOrderID);
                    return await command.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        /// <summary>
        /// Retrieves all order checkpoints for a specified tracked order
        /// </summary>
        /// <param name="trackedOrderID">ID of the tracked order</param>
        /// <returns>List of order checkpoints</returns>
        public async Task<List<OrderCheckpoint>> GetAllOrderCheckpointsAsync(int trackedOrderID)
        {
            List<OrderCheckpoint> checkpoints = new List<OrderCheckpoint>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(SELECT_ALL_ORDER_CHECKPOINTS, connection))
                {
                    command.Parameters.AddWithValue("@trackedOrderID", trackedOrderID);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            checkpoints.Add(new OrderCheckpoint
                            {
                                CheckpointID = reader.GetInt32(reader.GetOrdinal(DbColumns.CheckpointID)),
                                Timestamp = reader.GetDateTime(reader.GetOrdinal(DbColumns.Timestamp)),
                                Location = reader.IsDBNull(reader.GetOrdinal(DbColumns.Location)) ? null : reader.GetString(reader.GetOrdinal(DbColumns.Location)),
                                Description = reader.GetString(reader.GetOrdinal(DbColumns.Description)),
                                Status = Enum.Parse<OrderStatus>(reader.GetString(reader.GetOrdinal(DbColumns.CheckpointStatus))),
                                TrackedOrderID = reader.GetInt32(reader.GetOrdinal(DbColumns.TrackedOrderID))
                            });
                        }
                    }
                }
            }
            return checkpoints;
        }

        /// <summary>
        /// Retrieves all tracked orders in the system
        /// </summary>
        /// <returns>List of all tracked orders</returns>
        public async Task<List<TrackedOrder>> GetAllTrackedOrdersAsync()
        {
            List<TrackedOrder> orders = new List<TrackedOrder>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(SELECT_ALL_TRACKED_ORDERS, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(new TrackedOrder
                            {
                                TrackedOrderID = reader.GetInt32(reader.GetOrdinal(DbColumns.TrackedOrderID)),
                                OrderID = reader.GetInt32(reader.GetOrdinal(DbColumns.OrderID)),
                                CurrentStatus = Enum.Parse<OrderStatus>(reader.GetString(reader.GetOrdinal(DbColumns.OrderStatus))),
                                EstimatedDeliveryDate = reader.GetFieldValue<DateOnly>(reader.GetOrdinal(DbColumns.EstimatedDeliveryDate)),
                                DeliveryAddress = reader.GetString(reader.GetOrdinal(DbColumns.DeliveryAddress))
                            });
                        }
                    }
                }
            }
            return orders;
        }

        /// <summary>
        /// Retrieves an order checkpoint by its ID
        /// </summary>
        /// <param name="checkpointID">ID of the checkpoint to retrieve</param>
        /// <returns>The specified order checkpoint</returns>
        /// <exception cref="Exception">Thrown when the checkpoint is not found</exception>
        public async Task<OrderCheckpoint> GetOrderCheckpointByIdAsync(int checkpointID)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(SELECT_ORDER_CHECKPOINT_BY_ID, connection))
                {
                    command.Parameters.AddWithValue("@checkpointID", checkpointID);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new OrderCheckpoint
                            {
                                CheckpointID = reader.GetInt32(reader.GetOrdinal(DbColumns.CheckpointID)),
                                Timestamp = reader.GetDateTime(reader.GetOrdinal(DbColumns.Timestamp)),
                                Location = reader.IsDBNull(reader.GetOrdinal(DbColumns.Location)) ? null : reader.GetString(reader.GetOrdinal(DbColumns.Location)),
                                Description = reader.GetString(reader.GetOrdinal(DbColumns.Description)),
                                Status = Enum.Parse<OrderStatus>(reader.GetString(reader.GetOrdinal(DbColumns.CheckpointStatus))),
                                TrackedOrderID = reader.GetInt32(reader.GetOrdinal(DbColumns.TrackedOrderID))
                            };
                        }
                    }
                }
            }
            throw new Exception($"No OrderCheckpoint with id: {checkpointID}");
        }

        /// <summary>
        /// Retrieves a tracked order by its ID
        /// </summary>
        /// <param name="trackOrderID">ID of the tracked order to retrieve</param>
        /// <returns>The specified tracked order</returns>
        /// <exception cref="Exception">Thrown when the tracked order is not found</exception>
        public async Task<TrackedOrder> GetTrackedOrderByIdAsync(int trackOrderID)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(SELECT_TRACKED_ORDER_BY_ID, connection))
                {
                    command.Parameters.AddWithValue("@trackedOrderID", trackOrderID);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new TrackedOrder
                            {
                                TrackedOrderID = reader.GetInt32(reader.GetOrdinal(DbColumns.TrackedOrderID)),
                                OrderID = reader.GetInt32(reader.GetOrdinal(DbColumns.OrderID)),
                                CurrentStatus = Enum.Parse<OrderStatus>(reader.GetString(reader.GetOrdinal(DbColumns.OrderStatus))),
                                EstimatedDeliveryDate = reader.GetFieldValue<DateOnly>(reader.GetOrdinal(DbColumns.EstimatedDeliveryDate)),
                                DeliveryAddress = reader.GetString(reader.GetOrdinal(DbColumns.DeliveryAddress))
                            };
                        }
                    }
                }
            }
            throw new Exception($"No TrackedOrder with id: {trackOrderID}");
        }

        /// <summary>
        /// Updates an existing order checkpoint with new information
        /// </summary>
        /// <param name="checkpointID">ID of the checkpoint to update</param>
        /// <param name="timestamp">New timestamp for the checkpoint</param>
        /// <param name="location">New location for the checkpoint</param>
        /// <param name="description">New description for the checkpoint</param>
        /// <param name="status">New status for the checkpoint</param>
        public async Task UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string? location, string description, OrderStatus status)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(SP_UPDATE_ORDER_CHECKPOINT, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@timestamp", timestamp);
                    command.Parameters.AddWithValue("@location", location ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@description", description);
                    command.Parameters.AddWithValue("@checkpointStatus", status.ToString());
                    command.Parameters.AddWithValue("@checkpointID", checkpointID);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Updates an existing tracked order with new information
        /// </summary>
        /// <param name="trackedOrderID">ID of the tracked order to update</param>
        /// <param name="estimatedDeliveryDate">New estimated delivery date</param>
        /// <param name="currentStatus">New order status</param>
        public async Task UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, OrderStatus currentStatus)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(SP_UPDATE_TRACKED_ORDER, connection))
                {
                    DateTime estimatedDeliveryDateTime = estimatedDeliveryDate.ToDateTime(TimeOnly.MinValue);

                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@estimatedDeliveryDate", SqlDbType.Date).Value = estimatedDeliveryDateTime;
                    command.Parameters.AddWithValue("@orderStatus", currentStatus.ToString());
                    command.Parameters.AddWithValue("@trackedOrderID", trackedOrderID);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}