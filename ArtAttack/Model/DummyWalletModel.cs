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
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        }

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
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }
}
