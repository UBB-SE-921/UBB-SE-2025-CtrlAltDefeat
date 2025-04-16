using System;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Repository
{
    /// <summary>
    /// Provides database operations for order summary management.
    /// </summary>
    public class OrderSummaryRepository : IOrderSummaryRepository
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderSummaryRepository"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public OrderSummaryRepository(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderSummaryRepository"/> class with a specified database provider.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="databaseProvider">The database provider to use.</param>
        public OrderSummaryRepository(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<OrderSummary> GetOrderSummaryByIdAsync(int orderSummaryId)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "SELECT * FROM [OrderSummary] WHERE [ID] = @ID";

                    AddParameter(databaseCommand, "@ID", orderSummaryId);

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