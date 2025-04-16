using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Model
{
    public class ContractRepository : IContractRepository
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        public ContractRepository(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        public ContractRepository(string connectionString, IDatabaseProvider databaseProvider)
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
        /// Asynchronously retrieves a predefined contract by predefined contract type using the GetPredefinedContractByID stored procedure.
        /// </summary>
        /// <param name="predefinedContractType">The type of predefined contract to retrieve.</param>
        /// <returns>The predefined contract.</returns>
        public async Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType)
        {
            IPredefinedContract predefinedContract = null;

            using (var databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (var databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "GetPredefinedContractByID";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@PContractID", (int)predefinedContractType);

                    await databaseConnection.OpenAsync();
                    using (var reader = await databaseCommand.ExecuteReaderAsync())
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
            if (predefinedContract == null)
            {
                return new PredefinedContract
                {
                    ContractID = 0,
                    ContractContent = string.Empty
                };
            }

            return predefinedContract;
        }

        /// <summary>
        /// Asynchronously retrieves a single contract using the GetContractByID stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve.</param>
        /// <returns>The contract.</returns>
        public async Task<IContract> GetContractByIdAsync(long contractId)
        {
            IContract contract = null;

            using (var databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (var databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "GetContractByID";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@ContractID", contractId);

                    await databaseConnection.OpenAsync();
                    using (var reader = await databaseCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            contract = new Contract
                            {
                                ContractID = reader.GetInt32(reader.GetOrdinal("ID")),
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

            if (contract == null)
            {
                return new Contract();
            }

            return contract;
        }

        /// <summary>
        /// Asynchronously retrieves all contracts using the GetAllContracts stored procedure.
        /// </summary>
        /// <returns>The list of contracts.</returns>
        public async Task<List<IContract>> GetAllContractsAsync()
        {
            var contracts = new List<IContract>();

            using (var databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (var databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "GetAllContracts";
                    databaseCommand.CommandType = CommandType.StoredProcedure;

                    await databaseConnection.OpenAsync();
                    using (var reader = await databaseCommand.ExecuteReaderAsync())
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

            using (var databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (var databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "GetContractHistory";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@ContractID", contractId);

                    await databaseConnection.OpenAsync();
                    using (var reader = await databaseCommand.ExecuteReaderAsync())
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
            IContract newContract = new Contract();

            using (var databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (var databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "AddContract";
                    databaseCommand.CommandType = CommandType.StoredProcedure;

                    databaseCommand.Parameters.AddWithValue("@OrderID", contract.OrderID);
                    databaseCommand.Parameters.AddWithValue("@ContractStatus", contract.ContractStatus);
                    databaseCommand.Parameters.AddWithValue("@ContractContent", contract.ContractContent);
                    databaseCommand.Parameters.AddWithValue("@RenewalCount", contract.RenewalCount);
                    databaseCommand.Parameters.AddWithValue("@PredefinedContractID",
                        contract.PredefinedContractID.HasValue ? (object)contract.PredefinedContractID.Value : DBNull.Value);
                    databaseCommand.Parameters.AddWithValue("@PDFID", contract.PDFID);
                    databaseCommand.Parameters.AddWithValue("@PDFFile", pdfFile);
                    databaseCommand.Parameters.AddWithValue("@RenewedFromContractID",
                        contract.RenewedFromContractID.HasValue ? (object)contract.RenewedFromContractID.Value : DBNull.Value);

                    await databaseConnection.OpenAsync();
                    using (var reader = await databaseCommand.ExecuteReaderAsync())
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

            return newContract;
        }

        /// <summary>
        /// Asynchronously retrieves seller information for a given contract using the GetContractSeller stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the seller information for.</param>
        /// <returns>The seller information.</returns>
        public async Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId)
        {
            (int SellerID, string SellerName) sellerInfo = (0, string.Empty);

            using (var databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (var databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "GetContractSeller";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@ContractID", contractId);

                    await databaseConnection.OpenAsync();
                    using (var reader = await databaseCommand.ExecuteReaderAsync())
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

            using (var databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (var databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "GetContractBuyer";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@ContractID", contractId);

                    await databaseConnection.OpenAsync();
                    using (var reader = await databaseCommand.ExecuteReaderAsync())
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

            using (var databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (var databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "GetOrderSummaryInformation";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@ContractID", contractId);

                    await databaseConnection.OpenAsync();
                    using (var reader = await databaseCommand.ExecuteReaderAsync())
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
        public async Task<(DateTime? StartDate, DateTime? EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId)
        {
            using (var databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (var databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "GetProductDetailsByContractID";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@ContractID", contractId);

                    await databaseConnection.OpenAsync();
                    using (var reader = await databaseCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int startDateOrdinal = reader.GetOrdinal("startDate");
                            int endDateOrdinal = reader.GetOrdinal("endDate");
                            int priceOrdinal = reader.GetOrdinal("price");
                            int nameOrdinal = reader.GetOrdinal("name");

                            var startDate = reader.IsDBNull(startDateOrdinal) ? (DateTime?)null : reader.GetDateTime(startDateOrdinal);
                            var endDate = reader.IsDBNull(endDateOrdinal) ? (DateTime?)null : reader.GetDateTime(endDateOrdinal);
                            // Assuming price and name are non-nullable based on the original code and exception.
                            // Add null checks if these can also be null in the database.
                            var price = reader.GetDouble(priceOrdinal);
                            var name = reader.GetString(nameOrdinal);

                            return (startDate, endDate, price, name);
                        }
                    }
                }
            }

            return default; // Return null if no record found
        }

        /// <summary>
        /// Asynchronously retrieves all contracts for a given buyer using the GetContractsByBuyer stored procedure.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer to retrieve the contracts for.</param>
        /// <returns>The list of contracts.</returns>
        public async Task<List<IContract>> GetContractsByBuyerAsync(int buyerId)
        {
            var contracts = new List<IContract>();

            using (var databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (var databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "GetContractsByBuyer";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@BuyerID", buyerId);

                    await databaseConnection.OpenAsync();
                    using (var reader = await databaseCommand.ExecuteReaderAsync())
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
        /// <returns>The order details, with PaymentMethod potentially null.</returns>
        // Change return type to allow nullable PaymentMethod
        public async Task<(string? PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId)
        {
            string? paymentMethod = null; // Use nullable string
            DateTime orderDate = default;

            using (var databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (var databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "GetOrderDetails";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@ContractID", contractId);

                    await databaseConnection.OpenAsync();
                    using (var reader = await databaseCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int paymentMethodOrdinal = reader.GetOrdinal("PaymentMethod");
                            int orderDateOrdinal = reader.GetOrdinal("OrderDate");

                            // Check for DBNull before casting PaymentMethod
                            if (!reader.IsDBNull(paymentMethodOrdinal))
                            {
                                paymentMethod = reader.GetString(paymentMethodOrdinal);
                            }
                            // Assuming OrderDate is never null based on original code. Add check if needed.
                            orderDate = reader.GetDateTime(orderDateOrdinal);
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

            using (var databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (var databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "GetDeliveryDateByContractID";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@ContractID", contractId);

                    await databaseConnection.OpenAsync();
                    using (var reader = await databaseCommand.ExecuteReaderAsync())
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

            using (var databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (var databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "GetPdfByContractID";
                    databaseCommand.CommandType = CommandType.StoredProcedure;
                    databaseCommand.Parameters.AddWithValue("@ContractID", contractId);

                    await databaseConnection.OpenAsync();
                    using (var reader = await databaseCommand.ExecuteReaderAsync())
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