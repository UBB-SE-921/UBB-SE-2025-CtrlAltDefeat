using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Model
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
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UpdateWalletBalance";
                    cmd.CommandType = CommandType.StoredProcedure;

                    AddParameter(cmd, "@id", walletID);
                    AddParameter(cmd, "@balance", balance);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
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
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetWalletBalance";
                    cmd.CommandType = CommandType.StoredProcedure;

                    AddParameter(cmd, "@id", walletID);
                    await conn.OpenAsync();

                    using (IDataReader reader = await cmd.ExecuteReaderAsync())
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
