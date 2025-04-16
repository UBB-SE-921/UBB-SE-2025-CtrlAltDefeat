using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Repository
{
    public class DummyWalletModel : IDummyWalletModel
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        [ExcludeFromCodeCoverage]
        public DummyWalletModel(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        public DummyWalletModel(string connectionString, IDatabaseProvider databaseProvider)
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
        /// Updates the balance of a wallet in the database
        /// </summary>
        /// <param name="walletID">Id of the wallet to be updated</param>
        /// <param name="balance">Amount to update to</param>
        /// <returns></returns>
        public async Task UpdateWalletBalance(int walletID, float balance)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "UpdateWalletBalance";
                    databaseCommand.CommandType = CommandType.StoredProcedure;

                    AddParameter(databaseCommand, "@id", walletID);
                    AddParameter(databaseCommand, "@balance", balance);

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Retrieves the balance of a wallet from the database
        /// </summary>
        /// <param name="walletID">Id of the wallet to retrieve the balance of</param>
        /// <returns></returns>
        public async Task<float> GetWalletBalanceAsync(int walletID)
        {
            float walletBalance = -1;
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "GetWalletBalance";
                    databaseCommand.CommandType = CommandType.StoredProcedure;

                    AddParameter(databaseCommand, "@id", walletID);
                    await databaseConnection.OpenAsync();

                    using (IDataReader reader = await databaseCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            walletBalance = (float)reader.GetDouble(reader.GetOrdinal("balance"));
                        }
                    }
                }
            }
            return walletBalance;
        }

        [ExcludeFromCodeCoverage]
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
