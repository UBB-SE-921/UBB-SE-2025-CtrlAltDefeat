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
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("AddRenewedContract", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    AddContractParameters(command, contract);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private static void AddContractParameters(SqlCommand command, Contract contract)
        {
            command.Parameters.AddWithValue("@OrderID", contract.OrderID);
            command.Parameters.AddWithValue("@ContractContent", contract.ContractContent);
            command.Parameters.AddWithValue("@RenewalCount", contract.RenewalCount);
            command.Parameters.AddWithValue("@PDFID", contract.PDFID);

            command.Parameters.AddWithValue("@PredefinedContractID", contract.PredefinedContractID.HasValue ? (object)contract.PredefinedContractID.Value : DBNull.Value);
            command.Parameters.AddWithValue("@RenewedFromContractID", contract.RenewedFromContractID.HasValue ? (object)contract.RenewedFromContractID.Value : DBNull.Value);
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
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string query = "SELECT COUNT(*) FROM Contract WHERE RenewedFromContractID = @ContractID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ContractID", contractId);
                    await connection.OpenAsync();
                    int count = (int)await command.ExecuteScalarAsync();
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
            var contracts = new List<Contract>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("GetRenewedContracts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
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

        private static Contract MapContract(SqlDataReader reader)
        {
            return new Contract
            {
                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                OrderID = reader.GetInt32(reader.GetOrdinal("orderID")),
                ContractStatus = reader.GetString(reader.GetOrdinal("contractStatus")),
                ContractContent = reader["contractContent"] as string,
                RenewalCount = reader.GetInt32(reader.GetOrdinal("renewalCount")),
                PredefinedContractID = reader["predefinedContractID"] != DBNull.Value ? (int?)reader.GetInt32(reader.GetOrdinal("predefinedContractID")) : null,
                PDFID = reader.GetInt32(reader.GetOrdinal("pdfID")),
                RenewedFromContractID = reader["renewedFromContractID"] != DBNull.Value ? (int?)reader.GetInt32(reader.GetOrdinal("renewedFromContractID")) : null
            };
        }
    }
}
