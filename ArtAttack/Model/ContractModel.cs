using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using Microsoft.Data.SqlClient;

namespace ArtAttack.Model
{
    public class ContractModel : IContractModel
    {
        private readonly string connectionString;

        public ContractModel(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Asynchronously retrieves a predefined contract by predefined contract type using the GetPredefinedContractByID stored procedure.
        /// </summary>
        /// <param name="predefinedContractType" type="PredefinedContractType">The type of predefined contract to retrieve.</param>
        /// <returns type="Task<PredefinedContract>">The predefined contract.</returns>
        public async Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType)
        {
            IPredefinedContract predefinedContract = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetPredefinedContractByID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Pass the ID from the predefinedContract to the stored procedure parameter.
                    cmd.Parameters.Add("@PContractID", SqlDbType.BigInt).Value = predefinedContractType;
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows && await reader.ReadAsync())
                        {
                            predefinedContract = new PredefinedContract
                            {
                                ID = reader.GetInt32("ID"),
                                Content = reader["content"] as string
                            };
                        }
                    }
                }
            }
            return predefinedContract ?? new PredefinedContract { Content = string.Empty };
        }

        /// <summary>
        /// Asynchronously retrieves a single contract using the GetContractByID stored procedure.
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract to retrieve.</param>
        /// <returns type="Task<Contract>">The contract.</returns>
        public async Task<IContract> GetContractByIdAsync(long contractId)
        {
            IContract contract = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetContractByID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ContractID", SqlDbType.BigInt).Value = contractId;
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows && await reader.ReadAsync())
                        {
                            contract = new Contract
                            {
                                ContractID = reader.GetInt32("ID"),
                                OrderID = reader.GetInt32("orderID"),
                                ContractStatus = reader.GetString("contractStatus"),
                                ContractContent = reader["contractContent"] as string,
                                RenewalCount = reader.GetInt32("renewalCount"),
                                PredefinedContractID = reader["predefinedContractID"] != DBNull.Value
                                    ? (int?)reader.GetInt32("predefinedContractID")
                                    : null,
                                PDFID = reader.GetInt32("pdfID"),
                                RenewedFromContractID = reader["renewedFromContractID"] != DBNull.Value
                                    ? (long?)reader.GetInt64("renewedFromContractID")
                                    : null
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
        /// <returns type="Task<List<Contract>>">The list of contracts.</returns>
        public async Task<List<IContract>> GetAllContractsAsync()
        {
            var contracts = new List<IContract>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetAllContracts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var contract = new Contract
                            {
                                ContractID = reader.GetInt64(reader.GetOrdinal("ID")),
                                OrderID = reader.GetInt32(reader.GetOrdinal("orderID")),
                                ContractStatus = reader.GetString(reader.GetOrdinal("contractStatus")),
                                ContractContent = reader["contractContent"] as string,
                                RenewalCount = reader.GetInt32(reader.GetOrdinal("renewalCount")),
                                PredefinedContractID = reader["predefinedContractID"] != DBNull.Value
                                    ? (int?)reader.GetInt32(reader.GetOrdinal("predefinedContractID"))
                                    : null,
                                PDFID = reader.GetInt32(reader.GetOrdinal("pdfID")),
                                RenewedFromContractID = reader["renewedFromContractID"] != DBNull.Value
                                    ? (long?)reader.GetInt64(reader.GetOrdinal("renewedFromContractID"))
                                    : null
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
        /// <param name="contractId" type="long">The ID of the contract to retrieve the history for.</param>
        /// <returns type="Task<List<Contract>>">The list of contracts.</returns>
        public async Task<List<IContract>> GetContractHistoryAsync(long contractId)
        {
            var history = new List<IContract>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetContractHistory", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContractID", contractId);
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var contract = new Contract
                            {
                                ContractID = reader.GetInt64(reader.GetOrdinal("ID")),
                                OrderID = reader.GetInt32(reader.GetOrdinal("orderID")),
                                ContractStatus = reader.GetString(reader.GetOrdinal("contractStatus")),
                                ContractContent = reader["contractContent"] as string,
                                RenewalCount = reader.GetInt32(reader.GetOrdinal("renewalCount")),
                                PredefinedContractID = reader["predefinedContractID"] != DBNull.Value
                                    ? (int?)reader.GetInt32(reader.GetOrdinal("predefinedContractID"))
                                    : null,
                                PDFID = reader.GetInt32(reader.GetOrdinal("pdfID")),
                                RenewedFromContractID = reader["renewedFromContractID"] != DBNull.Value
                                    ? (long?)reader.GetInt64(reader.GetOrdinal("renewedFromContractID"))
                                    : null
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
        /// <param name="pdfFile" type="byte[]">The PDF file to update.</param>
        /// <param name="contract" type="Contract">The contract to insert.</param>
        /// <returns type="Task<Contract>">The new contract.</returns>
        public async Task<IContract> AddContractAsync(IContract contract, byte[] pdfFile)
        {
            IContract newContract = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("AddContract", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@OrderID", contract.OrderID);
                    cmd.Parameters.AddWithValue("@ContractStatus", contract.ContractStatus);
                    cmd.Parameters.AddWithValue("@ContractContent", contract.ContractContent);
                    cmd.Parameters.AddWithValue("@RenewalCount", contract.RenewalCount);

                    if (contract.PredefinedContractID.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@PredefinedContractID", contract.PredefinedContractID.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@PredefinedContractID", DBNull.Value);
                    }

                    cmd.Parameters.AddWithValue("@PDFID", contract.PDFID);
                    cmd.Parameters.AddWithValue("@PDFFile", pdfFile);

                    if (contract.RenewedFromContractID.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@RenewedFromContractID", contract.RenewedFromContractID.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@RenewedFromContractID", DBNull.Value);
                    }

                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            newContract = new Contract
                            {
                                ContractID = reader.GetInt64(reader.GetOrdinal("ID")),
                                OrderID = reader.GetInt32(reader.GetOrdinal("orderID")),
                                ContractStatus = reader.GetString(reader.GetOrdinal("contractStatus")),
                                ContractContent = reader["contractContent"] as string,
                                RenewalCount = reader.GetInt32(reader.GetOrdinal("renewalCount")),
                                PredefinedContractID = reader["predefinedContractID"] != DBNull.Value
                                                        ? (int?)reader.GetInt32(reader.GetOrdinal("predefinedContractID"))
                                                        : null,
                                PDFID = reader.GetInt32(reader.GetOrdinal("pdfID")),
                                RenewedFromContractID = reader["renewedFromContractID"] != DBNull.Value
                                                        ? (long?)reader.GetInt64(reader.GetOrdinal("renewedFromContractID"))
                                                        : null
                            };
                        }
                    }
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            return newContract ?? new Contract();
        }

        /// <summary>
        /// Asynchronously retrieves seller information for a given contract using the GetContractSeller stored procedure.
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract to retrieve the seller information for.</param>
        /// <returns type="(int SellerID, string SellerName)">The seller information.</returns>
        public async Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId)
        {
            (int SellerID, string SellerName) sellerInfo = (0, string.Empty);
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetContractSeller", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContractID", contractId);
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
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
        /// <param name="contractId" type="long">The ID of the contract to retrieve the buyer information for.</param>
        /// <returns type="(int BuyerID, string BuyerName)">The buyer information.</returns>
        public async Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId)
        {
            (int BuyerID, string BuyerName) buyerInfo = (0, string.Empty);
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetContractBuyer", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContractID", contractId);
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
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
        /// <param name="contractId" type="long">The ID of the contract to retrieve the order summary information for.</param>
        /// <returns type="Dictionary<string, object)">The order summary information.</returns>
        public async Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId)
        {
            var orderSummary = new Dictionary<string, object>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetOrderSummaryInformation", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContractID", contractId);
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
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
        /// <param name="contractId" type="long">The ID of the contract to retrieve the product details for.</param>
        /// <returns type="(DateTime StartDate, DateTime EndDate, double price, string name)?">The product details.</returns>
        public async Task<(DateTime StartDate, DateTime EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetProductDetailsByContractID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContractID", contractId);
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
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
        /// <param name="buyerId" type="int">The ID of the buyer to retrieve the contracts for.</param>
        /// <returns type="Task<List<Contract>>">The list of contracts.</returns>
        public async Task<List<IContract>> GetContractsByBuyerAsync(int buyerId)
        {
            var contracts = new List<IContract>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetContractsByBuyer", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BuyerID", SqlDbType.Int).Value = buyerId;
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var contract = new Contract
                            {
                                ContractID = reader.GetInt64(reader.GetOrdinal("ID")),
                                OrderID = reader.GetInt32(reader.GetOrdinal("orderID")),
                                ContractStatus = reader.GetString(reader.GetOrdinal("contractStatus")),
                                ContractContent = reader["contractContent"] as string,
                                RenewalCount = reader.GetInt32(reader.GetOrdinal("renewalCount")),
                                PredefinedContractID = reader["predefinedContractID"] != DBNull.Value ? (int?)reader.GetInt32(reader.GetOrdinal("predefinedContractID")) : null,
                                PDFID = reader.GetInt32(reader.GetOrdinal("pdfID")),
                                AdditionalTerms = reader.GetString(reader.GetOrdinal("AdditionalTerms")),
                                RenewedFromContractID = reader["renewedFromContractID"] != DBNull.Value ? (long?)reader.GetInt64(reader.GetOrdinal("renewedFromContractID")) : null
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
        /// <param name="contractId" type="long">The ID of the contract to retrieve the order details for.</param>
        /// <returns type="(string PaymentMethod, DateTime OrderDate)">The order details.</returns>
        public async Task<(string PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId)
        {
            (string PaymentMethod, DateTime OrderDate) details = (null, default);
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetOrderDetails", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ContractID", SqlDbType.Int).Value = contractId;
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string paymentMethod = reader["PaymentMethod"] as string;
                            var orderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate"));
                        }
                    }
                }
            }
            return details;
        }

        /// <summary>
        /// Asynchronously retrieves the delivery date for a given contract using the GetDeliveryDateByContractID stored procedure.
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract to retrieve the delivery date for.</param>
        /// <returns type="Task<DateTime?>">The delivery date.</returns>
        public async Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId)
        {
            DateTime? deliveryDate = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetDeliveryDateByContractID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ContractID", SqlDbType.Int).Value = contractId;
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
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
        /// <param name="contractId" type="long">The ID of the contract to retrieve the PDF file for.</param>
        /// <returns type="Task<byte[]>">The PDF file.</returns>
        public async Task<byte[]> GetPdfByContractIdAsync(long contractId)
        {
            byte[] pdfFile = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetPdfByContractID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ContractID", SqlDbType.BigInt).Value = contractId;
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            pdfFile = reader["PdfFile"] as byte[];
                        }
                    }
                }
            }
            return pdfFile;
        }
    }
}
