using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Model
{
    public class ContractRenewalModel : IContractRenewalModel
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        /// <summary>
        /// Initializes a new instance of the ContractRenewalModel class with default database provider.
        /// </summary>
        /// <param name="connectionString">The connection string to the database.</param>
        public ContractRenewalModel(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the ContractRenewalModel class.
        /// </summary>
        /// <param name="connectionString">The connection string to the database.</param>
        /// <param name="databaseProvider">The database provider.</param>
        public ContractRenewalModel(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        }

        /// <summary>
        /// Asynchronously adds a renewed contract to the database using the AddRenewedContract stored procedure.
        /// </summary>
        /// <param name="contract">The renewed contract to add to the database.</param>
        /// <param name="pdfFile">The PDF file of the renewed contract.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddRenewedContractAsync(IContract contract, byte[] pdfFile)
        {
            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "AddRenewedContract";
                    command.CommandType = CommandType.StoredProcedure;

                    AddContractParameters(command, contract, pdfFile);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private static void AddContractParameters(IDbCommand command, IContract contract, byte[] pdfFile = null)
        {
            command.Parameters.AddWithValue("@OrderID", contract.OrderID);
            command.Parameters.AddWithValue("@ContractStatus", contract.ContractStatus);
            command.Parameters.AddWithValue("@ContractContent", contract.ContractContent);
            command.Parameters.AddWithValue("@RenewalCount", contract.RenewalCount);
            command.Parameters.AddWithValue("@PDFID", contract.PDFID);

            if (pdfFile != null)
            {
                command.Parameters.AddWithValue("@PDFFile", pdfFile);
            }

            command.Parameters.AddWithValue("@PredefinedContractID",
                contract.PredefinedContractID.HasValue ? (object)contract.PredefinedContractID.Value : DBNull.Value);

            command.Parameters.AddWithValue("@RenewedFromContractID",
                contract.RenewedFromContractID.HasValue ? (object)contract.RenewedFromContractID.Value : DBNull.Value);
        }

        /// <summary>
        /// Asynchronously checks whether a contract has already been renewed by verifying
        /// if there exists any contract in the database with the given contract ID
        /// as its RenewedFromContractID.
        /// </summary>
        /// <param name="contractId">The ID of the contract to check.</param>
        /// <returns>A task representing the asynchronous operation. The task result is true if the contract has been renewed; otherwise, false.</returns>
        public async Task<bool> HasContractBeenRenewedAsync(long contractId)
        {
            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                const string query = "SELECT COUNT(*) FROM Contract WHERE RenewedFromContractID = @ContractID";
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@ContractID", contractId);

                    await connection.OpenAsync();
                    object result = await command.ExecuteScalarAsync();

                    return result != null && Convert.ToInt32(result) > 0;
                }
            }
        }

        /// <summary>
        /// Asynchronously retrieves all contracts with status 'RENEWED' using the GetRenewedContracts stored procedure.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The task result is a list of all renewed contracts.</returns>
        public async Task<List<IContract>> GetRenewedContractsAsync()
        {
            var contracts = new List<IContract>();

            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "GetRenewedContracts";
                    command.CommandType = CommandType.StoredProcedure;

                    await connection.OpenAsync();
                    using (IDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            contracts.Add(MapContract(reader));
                        }
                    }
                }
            }

            return contracts;
        }

        private static Contract MapContract(IDataReader reader)
        {
            return new Contract
            {
                ContractID = reader.GetInt64(reader.GetOrdinal("ID")),
                OrderID = reader.GetInt32(reader.GetOrdinal("orderID")),
                ContractStatus = reader.GetString(reader.GetOrdinal("contractStatus")),
                ContractContent = reader["contractContent"] as string,
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