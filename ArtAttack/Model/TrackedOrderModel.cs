using ArtAttack.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ArtAttack.Model
{
    class TrackedOrderModel
    {
        private readonly string connectionString;

        public TrackedOrderModel(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<int> AddOrderCheckpointAsync(OrderCheckpoint checkpoint)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("uspInsertOrderCheckpoint", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@timestamp", checkpoint.Timestamp);
                    cmd.Parameters.AddWithValue("@location", checkpoint.Location ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@description", checkpoint.Description);
                    cmd.Parameters.AddWithValue("@checkpointStatus", checkpoint.Status.ToString());
                    cmd.Parameters.AddWithValue("@trackedOrderID", checkpoint.TrackedOrderID);

                    SqlParameter outputParam = new SqlParameter("@newCheckpointID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    await cmd.ExecuteNonQueryAsync();

                    int newID = (int)cmd.Parameters["@newCheckpointID"].Value;
                    if (newID < 0)
                        throw new Exception("Unexpected error when trying to add the OrderCheckpoint");
                    return newID;
                }
            }
        }

        public async Task<int> AddTrackedOrderAsync(TrackedOrder order)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("uspInsertTrackedOrder", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@estimatedDeliveryDate", order.EstimatedDeliveryDate);
                    cmd.Parameters.AddWithValue("@deliveryAddress", order.DeliveryAddress);
                    cmd.Parameters.AddWithValue("@orderStatus", order.CurrentStatus.ToString());
                    cmd.Parameters.AddWithValue("@orderID", order.OrderID);

                    SqlParameter outputParam = new SqlParameter("@newTrackedOrderID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    await cmd.ExecuteNonQueryAsync();

                    int newID = (int)cmd.Parameters["@newTrackedOrderID"].Value;
                    if (newID < 0)
                        throw new Exception("Unexpected error when trying to add the TrackedOrder");
                    return newID;
                }
            }
        }

        public async Task<bool> DeleteOrderCheckpointAsync(int checkpointID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("uspDeleteOrderCheckpoint", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@checkpointID", checkpointID);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        public async Task<bool> DeleteTrackedOrderAsync(int trackOrderID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("uspDeleteTrackedOrder", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@trackOrderID", trackOrderID);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        public async Task<List<OrderCheckpoint>> GetAllOrderCheckpointsAsync(int trackedOrderID)
        {
            List<OrderCheckpoint> checkpoints = new List<OrderCheckpoint>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM OrderCheckpoints WHERE TrackedOrderID = @trackedOrderID", conn))
                {
                    cmd.Parameters.AddWithValue("@trackedOrderID", trackedOrderID);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            checkpoints.Add(new OrderCheckpoint
                            {
                                CheckpointID = reader.GetInt32(reader.GetOrdinal("CheckpointID")),
                                Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp")),
                                Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? null : reader.GetString(reader.GetOrdinal("Location")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                Status = Enum.Parse<OrderStatus>(reader.GetString(reader.GetOrdinal("CheckpointStatus"))),
                                TrackedOrderID = reader.GetInt32(reader.GetOrdinal("TrackedOrderID"))
                            });
                        }
                    }
                }
            }
            return checkpoints;
        }

        public async Task<List<TrackedOrder>> GetAllTrackedOrdersAsync()
        {
            List<TrackedOrder> orders = new List<TrackedOrder>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM TrackedOrders", conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(new TrackedOrder
                            {
                                TrackedOrderID = reader.GetInt32(reader.GetOrdinal("TrackedOrderID")),
                                OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                                CurrentStatus = Enum.Parse<OrderStatus>(reader.GetString(reader.GetOrdinal("OrderStatus"))),
                                EstimatedDeliveryDate = reader.GetFieldValue<DateOnly>(reader.GetOrdinal("EstimatedDeliveryDate")),
                                DeliveryAddress = reader.GetString(reader.GetOrdinal("DeliveryAddress"))
                            });
                        }
                    }
                }
            }
            return orders;
        }

        public async Task<OrderCheckpoint> GetOrderCheckpointByIdAsync(int checkpointID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM OrderCheckpoints WHERE CheckpointID = @checkpointID", conn))
                {
                    cmd.Parameters.AddWithValue("@checkpointID", checkpointID);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new OrderCheckpoint
                            {
                                CheckpointID = reader.GetInt32(reader.GetOrdinal("CheckpointID")),
                                Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp")),
                                Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? null : reader.GetString(reader.GetOrdinal("Location")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                Status = Enum.Parse<OrderStatus>(reader.GetString(reader.GetOrdinal("CheckpointStatus"))),
                                TrackedOrderID = reader.GetInt32(reader.GetOrdinal("TrackedOrderID"))
                            };
                        }
                    }
                }
            }
            throw new Exception("No OrderChekpoint with id: " + checkpointID.ToString());
        }

        public async Task<TrackedOrder> GetTrackedOrderByIdAsync(int trackOrderID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM TrackedOrders WHERE TrackedOrderID = @trackedOrderID", conn))
                {
                    cmd.Parameters.AddWithValue("@trackedOrderID", trackOrderID);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new TrackedOrder
                            {
                                TrackedOrderID = reader.GetInt32(reader.GetOrdinal("TrackedOrderID")),
                                OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                                CurrentStatus = Enum.Parse<OrderStatus>(reader.GetString(reader.GetOrdinal("OrderStatus"))),
                                EstimatedDeliveryDate = reader.GetFieldValue<DateOnly>(reader.GetOrdinal("EstimatedDeliveryDate")),
                                DeliveryAddress = reader.GetString(reader.GetOrdinal("DeliveryAddress"))
                            };
                        }
                    }
                }
            }
            throw new Exception("No TrackedOrder with id: " + trackOrderID.ToString());
        }

        public async Task UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string? location, string description, OrderStatus status)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("uspUpdateOrderCheckpoint", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@timestamp", timestamp);
                    cmd.Parameters.AddWithValue("@location", location ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@description", description);
                    cmd.Parameters.AddWithValue("@checkpointStatus", status.ToString());
                    cmd.Parameters.AddWithValue("@checkpointID", checkpointID);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, OrderStatus currentStatus)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("uspUpdateTrackedOrder", conn))
                {
                    DateTime estimatedDeliveryDateTime = estimatedDeliveryDate.ToDateTime(TimeOnly.MinValue);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@estimatedDeliveryDate", SqlDbType.Date).Value = estimatedDeliveryDateTime;
                    cmd.Parameters.AddWithValue("@orderStatus", currentStatus.ToString());
                    cmd.Parameters.AddWithValue("@trackedOrderID", trackedOrderID);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}