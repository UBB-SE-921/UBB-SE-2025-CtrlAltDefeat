using System;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Model
{
    public class OrderSummaryModel : IOrderSummaryModel
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        /// <summary>
        /// Default constructor that uses SQL Server implementation
        /// </summary>
        public OrderSummaryModel(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Constructor with dependency injection for testing
        /// </summary>
        public OrderSummaryModel(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
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
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "AddOrderSummary";

                    AddParameter(cmd, "@Subtotal", subtotal);
                    AddParameter(cmd, "@WarrantyTax", warrantyTax);
                    AddParameter(cmd, "@DeliveryFee", deliveryFee);
                    AddParameter(cmd, "@FinalTotal", finalTotal);
                    AddParameter(cmd, "@FullName", fullName);
                    AddParameter(cmd, "@Email", email);
                    AddParameter(cmd, "@PhoneNumber", phoneNumber);
                    AddParameter(cmd, "@Address", address);
                    AddParameter(cmd, "@PostalCode", postalCode);
                    AddParameter(cmd, "@AdditionalInfo", additionalInfo);
                    AddParameter(cmd, "@ContractDetails", contractDetails);

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
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "UpdateOrderSummary";

                    AddParameter(cmd, "@ID", id);
                    AddParameter(cmd, "@Subtotal", subtotal);
                    AddParameter(cmd, "@WarrantyTax", warrantyTax);
                    AddParameter(cmd, "@DeliveryFee", deliveryFee);
                    AddParameter(cmd, "@FinalTotal", finalTotal);
                    AddParameter(cmd, "@FullName", fullName);
                    AddParameter(cmd, "@Email", email);
                    AddParameter(cmd, "@PhoneNumber", phoneNumber);
                    AddParameter(cmd, "@Address", address);
                    AddParameter(cmd, "@PostalCode", postalCode);
                    AddParameter(cmd, "@AdditionalInfo", additionalInfo);
                    AddParameter(cmd, "@ContractDetails", contractDetails);

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
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "DeleteOrderSummary";

                    AddParameter(cmd, "@ID", id);

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
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM [OrderSummary] WHERE [ID] = @ID";

                    AddParameter(cmd, "@ID", orderSummaryID);

                    await conn.OpenAsync();
                    using (IDataReader reader = await cmd.ExecuteReaderAsync())
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

        /// <summary>
        /// Helper method to add a parameter to a command
        /// </summary>
        private void AddParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }
}
