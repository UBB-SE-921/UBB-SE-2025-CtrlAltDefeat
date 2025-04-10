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
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (databaseProvider == null)
            {
                throw new ArgumentNullException(nameof(databaseProvider));
            }

            this.connectionString = connectionString;
            this.databaseProvider = databaseProvider;
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
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.CommandText = "AddOrderSummary";

                    AddParameter(databaseCommand, "@Subtotal", subtotal);
                    AddParameter(databaseCommand, "@WarrantyTax", warrantyTax);
                    AddParameter(databaseCommand, "@DeliveryFee", deliveryFee);
                    AddParameter(databaseCommand, "@FinalTotal", finalTotal);
                    AddParameter(databaseCommand, "@FullName", fullName);
                    AddParameter(databaseCommand, "@Email", email);
                    AddParameter(databaseCommand, "@PhoneNumber", phoneNumber);
                    AddParameter(databaseCommand, "@Address", address);
                    AddParameter(databaseCommand, "@PostalCode", postalCode);
                    AddParameter(databaseCommand, "@AdditionalInfo", additionalInfo);
                    AddParameter(databaseCommand, "@ContractDetails", contractDetails);

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
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
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.CommandText = "UpdateOrderSummary";

                    AddParameter(databaseCommand, "@ID", id);
                    AddParameter(databaseCommand, "@Subtotal", subtotal);
                    AddParameter(databaseCommand, "@WarrantyTax", warrantyTax);
                    AddParameter(databaseCommand, "@DeliveryFee", deliveryFee);
                    AddParameter(databaseCommand, "@FinalTotal", finalTotal);
                    AddParameter(databaseCommand, "@FullName", fullName);
                    AddParameter(databaseCommand, "@Email", email);
                    AddParameter(databaseCommand, "@PhoneNumber", phoneNumber);
                    AddParameter(databaseCommand, "@Address", address);
                    AddParameter(databaseCommand, "@PostalCode", postalCode);
                    AddParameter(databaseCommand, "@AdditionalInfo", additionalInfo);
                    AddParameter(databaseCommand, "@ContractDetails", contractDetails);

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
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
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.CommandText = "DeleteOrderSummary";

                    AddParameter(databaseCommand, "@ID", id);

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
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
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "SELECT * FROM [OrderSummary] WHERE [ID] = @ID";

                    AddParameter(databaseCommand, "@ID", orderSummaryID);

                    await databaseConnection.OpenAsync();
                    using (IDataReader reader = await databaseCommand.ExecuteReaderAsync())
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

            if (value == null)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.Value = value;
            }

            command.Parameters.Add(parameter);
        }
    }
}
