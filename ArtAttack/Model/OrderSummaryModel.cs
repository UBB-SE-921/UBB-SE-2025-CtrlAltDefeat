using System;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using Microsoft.Data.SqlClient;

namespace ArtAttack.Model
{
    public class OrderSummaryModel
    {
        private readonly string connectionString;

        public OrderSummaryModel(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Adds an order summary to the database using the AddOrderSummary stored procedure
        /// </summary>
        /// <param name="subtotal">The subtotal of the order</param>
        /// <param name="warrantyTax">The warranty tax of the order</param>
        /// <param name="deliveryFee">The delivery fee of the order</param>
        /// <param name="finalTotal">The final total of the order</param>
        /// <param name="fullName">The order's full name</param>
        /// <param name="email">The email on which the order was placed</param>
        /// <param name="phoneNumber">The phone number on which the order was placed</param>
        /// <param name="address">The order's address</param>
        /// <param name="postalCode">The postal code of the order</param>
        /// <param name="additionalInfo">Additional information for the order</param>
        /// <param name="contractDetails">Other contact information for the order</param>
        /// <returns></returns>
        public async Task AddOrderSummaryAsync(float subtotal, float warrantyTax, float deliveryFee, float finalTotal,
                                    string fullName, string email, string phoneNumber, string address,
                                    string postalCode, string additionalInfo, string contractDetails)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("AddOrderSummary", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                    cmd.Parameters.AddWithValue("@WarrantyTax", warrantyTax);
                    cmd.Parameters.AddWithValue("@DeliveryFee", deliveryFee);
                    cmd.Parameters.AddWithValue("@FinalTotal", finalTotal);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@PostalCode", postalCode);
                    cmd.Parameters.AddWithValue("@AdditionalInfo", additionalInfo);
                    cmd.Parameters.AddWithValue("@ContractDetails", contractDetails ?? (object)DBNull.Value);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Updates an order summary in the database using the UpdateOrderSummary stored procedure
        /// </summary>
        /// <param name="id">The id of the order to be updated</param>
        /// <param name="subtotal">The subtotal of the order</param>
        /// <param name="warrantyTax">The warranty tax of the order</param>
        /// <param name="deliveryFee">The delivery fee of the order</param>
        /// <param name="finalTotal">The final total of the order</param>
        /// <param name="fullName">The order's full name</param>
        /// <param name="email">The email on which the order was placed</param>
        /// <param name="phoneNumber">The phone number on which the order was placed</param>
        /// <param name="address">The order's address</param>
        /// <param name="postalCode">The postal code of the order</param>
        /// <param name="additionalInfo">Additional information for the order</param>
        /// <param name="contractDetails">Other contact information for the order</param>
        /// <returns></returns>
        public async Task UpdateOrderSummaryAsync(int id, float subtotal, float warrantyTax, float deliveryFee, float finalTotal,
                                       string fullName, string email, string phoneNumber, string address,
                                       string postalCode, string additionalInfo, string contractDetails)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("UpdateOrderSummary", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                    cmd.Parameters.AddWithValue("@WarrantyTax", warrantyTax);
                    cmd.Parameters.AddWithValue("@DeliveryFee", deliveryFee);
                    cmd.Parameters.AddWithValue("@FinalTotal", finalTotal);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@PostalCode", postalCode);
                    cmd.Parameters.AddWithValue("@AdditionalInfo", additionalInfo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ContractDetails", contractDetails ?? (object)DBNull.Value);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Deletes an order summary from the database using the DeleteOrderSummary stored procedure
        /// </summary>
        /// <param name="id">The id of the order summary to be deleted</param>
        /// <returns></returns>
        public async Task DeleteOrderSummaryAsync(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("DeleteOrderSummary", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Retrieves an order summary from the database using the GetOrderSummaryByID stored procedure
        /// </summary>
        /// <param name="orderSummaryID">The id of the order summary to be retrieved</param>
        /// <returns></returns>
        public async Task<OrderSummary> GetOrderSummaryByIDAsync(int orderSummaryID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("select * from [OrderSummary] where [ID] = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", orderSummaryID);

                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new OrderSummary
                            {
                                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                                Subtotal = (float)reader.GetDouble(reader.GetOrdinal("Subtotal")),
                                WarrantyTax = (float)reader.GetDouble(reader.GetOrdinal("WarrantyTax")),
                                DeliveryFee = (float)reader.GetDouble(reader.GetOrdinal("DeliveryFee")),
                                FinalTotal = (float)reader.GetDouble(reader.GetOrdinal("FinalTotal")),
                                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                PostalCode = reader.GetString(reader.GetOrdinal("PostalCode")),
                                AdditionalInfo = reader.IsDBNull(reader.GetOrdinal("AdditionalInfo")) ? null : reader.GetString(reader.GetOrdinal("AdditionalInfo")),
                                ContractDetails = reader.IsDBNull(reader.GetOrdinal("ContractDetails")) ? null : reader.GetString(reader.GetOrdinal("ContractDetails"))
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}
