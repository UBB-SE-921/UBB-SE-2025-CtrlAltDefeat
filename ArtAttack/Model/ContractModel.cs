using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Model
{
    public class ContractModel : IContractModel
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        public ContractModel(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        public ContractModel(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        }

        /// <summary>
        /// Asynchronously retrieves a predefined contract by predefined contract type using the GetPredefinedContractByID stored procedure.
        /// </summary>
        /// <param name="predefinedContractType">The type of predefined contract to retrieve.</param>
        /// <returns>The predefined contract.</returns>
        public async Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType)
        {
            IPredefinedContract predefinedContract = null;

            using (var conn = databaseProvider.CreateConnection(connectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetPredefinedContractByID";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PContractID", (int)predefinedContractType);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            predefinedContract = new PredefinedContract
                            {
                                ContractID = reader.GetInt32(reader.GetOrdinal("ID")),
                                ContractContent = (string)reader["content"]
                            };
                        }
                    }
                }
            }

            // If the contract is null, return a new instance with an empty content.
            return predefinedContract ?? new PredefinedContract
            {
                ContractID = 0,
                ContractContent = string.Empty
            };
        }

        /// <summary>
        /// Asynchronously retrieves a single contract using the GetContractByID stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve.</param>
        /// <returns>The contract.</returns>
        public async Task<IContract> GetContractByIdAsync(long contractId)
        {
            IContract contract = null;

            using (var conn = databaseProvider.CreateConnection(connectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetContractByID";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContractID", contractId);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            contract = new Contract
                            {
                                ContractID = reader.GetInt64(reader.GetOrdinal("ID")),
                                OrderID = reader.GetInt32(reader.GetOrdinal("orderID")),
                                ContractStatus = reader.GetString(reader.GetOrdinal("contractStatus")),
                                ContractContent = (string)reader["contractContent"],
                                RenewalCount = reader.GetInt32(reader.GetOrdinal("renewalCount")),
                                PredefinedContractID = reader.IsDBNull(reader.GetOrdinal("predefinedContractID"))
                                    ? null
                                    : (int?)reader.GetInt32(reader.GetOrdinal("predefinedContractID")),
                                PDFID = reader.GetInt32(reader.GetOrdinal("pdfID")),
                                RenewedFromContractID = reader.IsDBNull(reader.GetOrdinal("renewedFromContractID"))
                                    ? null
                                    : (long?)reader.GetInt64(reader.GetOrdinal("renewedFromContractID"))
                            };
                        }
                    }
                }
            }

            return contract ?? new Contract();
        }

        /// <summary>
        /// Asynchronously retrieves all contracts using the GetAllContracts stored procedure.
        /// </summary>
        /// <returns>The list of contracts.</returns>
        public async Task<List<IContract>> GetAllContractsAsync()
        {
            var contracts = new List<IContract>();

            using (var conn = databaseProvider.CreateConnection(connectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetAllContracts";
                    cmd.CommandType = CommandType.StoredProcedure;

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var contract = new Contract
                            {
                                ContractID = reader.GetInt64(reader.GetOrdinal("ID")),
                                OrderID = reader.GetInt32(reader.GetOrdinal("orderID")),
                                ContractStatus = reader.GetString(reader.GetOrdinal("contractStatus")),
                                ContractContent = (string)reader["contractContent"],
                                RenewalCount = reader.GetInt32(reader.GetOrdinal("renewalCount")),
                                PredefinedContractID = reader.IsDBNull(reader.GetOrdinal("predefinedContractID"))
                                    ? null
                                    : (int?)reader.GetInt32(reader.GetOrdinal("predefinedContractID")),
                                PDFID = reader.GetInt32(reader.GetOrdinal("pdfID")),
                                RenewedFromContractID = reader.IsDBNull(reader.GetOrdinal("renewedFromContractID"))
                                    ? null
                                    : (long?)reader.GetInt64(reader.GetOrdinal("renewedFromContractID"))
                            };
                            contracts.Add(contract);
                        }
                    }
                }
            }

            return contracts;
        }

        /// <summary>
        /// Asynchronously retrieves the renewal history for a contract using the GetContractHistory stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the history for.</param>
        /// <returns>The list of contracts.</returns>
        public async Task<List<IContract>> GetContractHistoryAsync(long contractId)
        {
            var history = new List<IContract>();

            using (var conn = databaseProvider.CreateConnection(connectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetContractHistory";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContractID", contractId);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var contract = new Contract
                            {
                                ContractID = reader.GetInt64(reader.GetOrdinal("ID")),
                                OrderID = reader.GetInt32(reader.GetOrdinal("orderID")),
                                ContractStatus = reader.GetString(reader.GetOrdinal("contractStatus")),
                                ContractContent = (string)reader["contractContent"],
                                RenewalCount = reader.GetInt32(reader.GetOrdinal("renewalCount")),
                                PredefinedContractID = reader.IsDBNull(reader.GetOrdinal("predefinedContractID"))
                                    ? null
                                    : (int?)reader.GetInt32(reader.GetOrdinal("predefinedContractID")),
                                PDFID = reader.GetInt32(reader.GetOrdinal("pdfID")),
                                RenewedFromContractID = reader.IsDBNull(reader.GetOrdinal("renewedFromContractID"))
                                    ? null
                                    : (long?)reader.GetInt64(reader.GetOrdinal("renewedFromContractID"))
                            };
                            history.Add(contract);
                        }
                    }
                }
            }

            return history;
        }

        /// <summary>
        /// Asynchronously inserts a new contract and updates the PDF file using the AddContract stored procedure.
        /// </summary>
        /// <param name="contract">The contract to insert.</param>
        /// <param name="pdfFile">The PDF file to update.</param>
        /// <returns>The new contract.</returns>
        public async Task<IContract> AddContractAsync(IContract contract, byte[] pdfFile)
        {
            IContract newContract = null;

            using (var conn = databaseProvider.CreateConnection(connectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "AddContract";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@OrderID", contract.OrderID);
                    cmd.Parameters.AddWithValue("@ContractStatus", contract.ContractStatus);
                    cmd.Parameters.AddWithValue("@ContractContent", contract.ContractContent);
                    cmd.Parameters.AddWithValue("@RenewalCount", contract.RenewalCount);
                    cmd.Parameters.AddWithValue("@PredefinedContractID",
                        contract.PredefinedContractID.HasValue ? (object)contract.PredefinedContractID.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@PDFID", contract.PDFID);
                    cmd.Parameters.AddWithValue("@PDFFile", pdfFile ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RenewedFromContractID",
                        contract.RenewedFromContractID.HasValue ? (object)contract.RenewedFromContractID.Value : DBNull.Value);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            newContract = new Contract
                            {
                                ContractID = reader.GetInt64(reader.GetOrdinal("ID")),
                                OrderID = reader.GetInt32(reader.GetOrdinal("orderID")),
                                ContractStatus = reader.GetString(reader.GetOrdinal("contractStatus")),
                                ContractContent = (string)reader["contractContent"],
                                RenewalCount = reader.GetInt32(reader.GetOrdinal("renewalCount")),
                                PredefinedContractID = reader.IsDBNull(reader.GetOrdinal("predefinedContractID"))
                                    ? null
                                    : (int?)reader.GetInt32(reader.GetOrdinal("predefinedContractID")),
                                PDFID = reader.GetInt32(reader.GetOrdinal("pdfID")),
                                RenewedFromContractID = reader.IsDBNull(reader.GetOrdinal("renewedFromContractID"))
                                    ? null
                                    : (long?)reader.GetInt64(reader.GetOrdinal("renewedFromContractID"))
                            };
                        }
                    }
                }
            }

            return newContract ?? new Contract();
        }

        /// <summary>
        /// Asynchronously retrieves seller information for a given contract using the GetContractSeller stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the seller information for.</param>
        /// <returns>The seller information.</returns>
        public async Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId)
        {
            (int SellerID, string SellerName) sellerInfo = (0, string.Empty);

            using (var conn = databaseProvider.CreateConnection(connectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetContractSeller";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContractID", contractId);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            sellerInfo = (
                                SellerID: reader.GetInt32(reader.GetOrdinal("SellerID")),
                                SellerName: reader.GetString(reader.GetOrdinal("SellerName")));
                        }
                    }
                }
            }

            return sellerInfo;
        }

        /// <summary>
        /// Asynchronously retrieves buyer information for a given contract using the GetContractBuyer stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the buyer information for.</param>
        /// <returns>The buyer information.</returns>
        public async Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId)
        {
            (int BuyerID, string BuyerName) buyerInfo = (0, string.Empty);

            using (var conn = databaseProvider.CreateConnection(connectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetContractBuyer";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContractID", contractId);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            buyerInfo = (
                                BuyerID: reader.GetInt32(reader.GetOrdinal("BuyerID")),
                                BuyerName: reader.GetString(reader.GetOrdinal("BuyerName")));
                        }
                    }
                }
            }

            return buyerInfo;
        }

        /// <summary>
        /// Asynchronously retrieves order summary information for a contract using the GetOrderSummaryInformation stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the order summary information for.</param>
        /// <returns>The order summary information.</returns>
        public async Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId)
        {
            var orderSummary = new Dictionary<string, object>();

            using (var conn = databaseProvider.CreateConnection(connectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetOrderSummaryInformation";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContractID", contractId);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            orderSummary["ID"] = reader["ID"];
                            orderSummary["subtotal"] = reader["subtotal"];
                            orderSummary["warrantyTax"] = reader["warrantyTax"];
                            orderSummary["deliveryFee"] = reader["deliveryFee"];
                            orderSummary["finalTotal"] = reader["finalTotal"];
                            orderSummary["fullName"] = reader["fullName"];
                            orderSummary["email"] = reader["email"];
                            orderSummary["phoneNumber"] = reader["phoneNumber"];
                            orderSummary["address"] = reader["address"];
                            orderSummary["postalCode"] = reader["postalCode"];
                            orderSummary["additionalInfo"] = reader["additionalInfo"];
                            orderSummary["ContractDetails"] = reader["ContractDetails"];
                        }
                    }
                }
            }

            return orderSummary;
        }

        /// <summary>
        /// Asynchronously retrieves the startDate and endDate for a contract from the DummyProduct table.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the product details for.</param>
        /// <returns>The product details.</returns>
        public async Task<(DateTime StartDate, DateTime EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId)
        {
            using (var conn = databaseProvider.CreateConnection(connectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetProductDetailsByContractID";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContractID", contractId);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var startDate = reader.GetDateTime(reader.GetOrdinal("startDate"));
                            var endDate = reader.GetDateTime(reader.GetOrdinal("endDate"));
                            var price = reader.GetDouble(reader.GetOrdinal("price"));
                            var name = reader.GetString(reader.GetOrdinal("name"));
                            return (startDate, endDate, price, name);
                        }
                    }
                }
            }

            return default;
        }

        /// <summary>
        /// Asynchronously retrieves all contracts for a given buyer using the GetContractsByBuyer stored procedure.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer to retrieve the contracts for.</param>
        /// <returns>The list of contracts.</returns>
        public async Task<List<IContract>> GetContractsByBuyerAsync(int buyerId)
        {
            var contracts = new List<IContract>();

            using (var conn = databaseProvider.CreateConnection(connectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetContractsByBuyer";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BuyerID", buyerId);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var contract = new Contract
                            {
                                ContractID = reader.GetInt64(reader.GetOrdinal("ID")),
                                OrderID = reader.GetInt32(reader.GetOrdinal("orderID")),
                                ContractStatus = reader.GetString(reader.GetOrdinal("contractStatus")),
                                ContractContent = (string)reader["contractContent"],
                                RenewalCount = reader.GetInt32(reader.GetOrdinal("renewalCount")),
                                PredefinedContractID = reader.IsDBNull(reader.GetOrdinal("predefinedContractID"))
                                    ? null
                                    : (int?)reader.GetInt32(reader.GetOrdinal("predefinedContractID")),
                                PDFID = reader.GetInt32(reader.GetOrdinal("pdfID")),
                                AdditionalTerms = reader.GetString(reader.GetOrdinal("AdditionalTerms")),
                                RenewedFromContractID = reader.IsDBNull(reader.GetOrdinal("renewedFromContractID"))
                                    ? null
                                    : (long?)reader.GetInt64(reader.GetOrdinal("renewedFromContractID"))
                            };
                            contracts.Add(contract);
                        }
                    }
                }
            }

            return contracts;
        }

        /// <summary>
        /// Asynchronously retrieves the payment method and order date for a given contract using the GetOrderDetails stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the order details for.</param>
        /// <returns>The order details.</returns>
        public async Task<(string PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId)
        {
            string paymentMethod = null;
            DateTime orderDate = default;

            using (var conn = databaseProvider.CreateConnection(connectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetOrderDetails";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContractID", contractId);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            paymentMethod = (string)reader["PaymentMethod"];
                            orderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate"));
                        }
                    }
                }
            }

            return (paymentMethod, orderDate);
        }

        /// <summary>
        /// Asynchronously retrieves the delivery date for a given contract using the GetDeliveryDateByContractID stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the delivery date for.</param>
        /// <returns>The delivery date.</returns>
        public async Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId)
        {
            DateTime? deliveryDate = null;

            using (var conn = databaseProvider.CreateConnection(connectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetDeliveryDateByContractID";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContractID", contractId);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int ordinal = reader.GetOrdinal("EstimatedDeliveryDate");
                            if (!reader.IsDBNull(ordinal))
                            {
                                deliveryDate = reader.GetDateTime(ordinal);
                            }
                        }
                    }
                }
            }

            return deliveryDate;
        }

        /// <summary>
        /// Asynchronously retrieves the PDF file for a given contract using the GetPdfByContractID stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the PDF file for.</param>
        /// <returns>The PDF file.</returns>
        public async Task<byte[]> GetPdfByContractIdAsync(long contractId)
        {
            byte[] pdfFile = null;

            using (var conn = databaseProvider.CreateConnection(connectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetPdfByContractID";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContractID", contractId);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            pdfFile = (byte[])reader["PdfFile"];
                        }
                    }
                }
            }

            return pdfFile;
        }
    }
}