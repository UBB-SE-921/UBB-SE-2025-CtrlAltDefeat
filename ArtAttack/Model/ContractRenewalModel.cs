using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ArtAttack.Model
{
    public class ContractRenewalModel : IContractRenewalModel
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the ContractRenewalModel class.
        /// </summary>
        /// <param name="connectionString" >The connection string to the database.</param>
        public ContractRenewalModel(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Asynchronously adds a renewed contract to the database using the AddRenewedContract stored procedure.
        /// </summary>
        /// <param name="contract">The renewed contract to add to the database.</param>
        /// <param name="pdfFile" >The PDF file of the renewed contract.</param>
        /// <returns >A task representing the asynchronous operation.</returns>
        public async Task AddRenewedContractAsync(IContract contract, byte[] pdfFile)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("AddRenewedContract", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@OrderID", contract.OrderID);
                    cmd.Parameters.AddWithValue("@ContractContent", contract.ContractContent);
                    cmd.Parameters.AddWithValue("@RenewalCount", contract.RenewalCount);
                    cmd.Parameters.AddWithValue("@PDFID", contract.PDFID);

                    if (contract.PredefinedContractID.HasValue)
                        cmd.Parameters.AddWithValue("@PredefinedContractID", contract.PredefinedContractID.Value);
                    else
                        cmd.Parameters.AddWithValue("@PredefinedContractID", DBNull.Value);

                    if (contract.RenewedFromContractID.HasValue)
                        cmd.Parameters.AddWithValue("@RenewedFromContractID", contract.RenewedFromContractID.Value);
                    else
                        cmd.Parameters.AddWithValue("@RenewedFromContractID", DBNull.Value);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }


        /// <summary>
        /// Asynchronously checks whether a contract has already been renewed by verifying 
        /// if there exists any contract in the database with the given contract ID 
        /// as its RenewedFromContractID.
        /// </summary>
        /// <param name="contractId" >The ID of the contract to check.</param>
        /// <returns >A task representing the asynchronous operation. The task result is true if the contract has been renewed; otherwise, false.</returns>
        public async Task<bool> HasContractBeenRenewedAsync(long contractId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM Contract WHERE RenewedFromContractID = @ContractID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ContractID", contractId);
                    await conn.OpenAsync();
                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }


        /// <summary>
        /// Asynchronously retrieves all contracts with status 'RENEWED' using the GetRenewedContracts stored procedure.
        /// </summary>
        /// <returns >A task representing the asynchronous operation. The task result is a list of all renewed contracts.</returns>
        public async Task<List<IContract>> GetRenewedContractsAsync()
        {
            var contracts = new List<IContract>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetRenewedContracts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var contract = new Contract
                            {
                                ContractID = reader.GetInt32(reader.GetOrdinal("ID")),
                                OrderID = reader.GetInt32(reader.GetOrdinal("orderID")),
                                ContractStatus = reader.GetString(reader.GetOrdinal("contractStatus")),
                                ContractContent = reader["contractContent"] as string,
                                RenewalCount = reader.GetInt32(reader.GetOrdinal("renewalCount")),
                                PredefinedContractID = reader["predefinedContractID"] != DBNull.Value
                                    ? (int?)reader.GetInt32(reader.GetOrdinal("predefinedContractID"))
                                    : null,
                                PDFID = reader.GetInt32(reader.GetOrdinal("pdfID")),
                                RenewedFromContractID = reader["renewedFromContractID"] != DBNull.Value
                                    ? (int?)reader.GetInt32(reader.GetOrdinal("renewedFromContractID"))
                                    : null
                            };
                            contracts.Add(contract);
                        }
                    }
                }
            }
            return contracts;
        }
    }
}
