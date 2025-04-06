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
