using System;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Model
{
    public class DummyCardModel : IDummyCardModel
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        public DummyCardModel(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        public DummyCardModel(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        }

        public async Task DeleteCardAsync(string cardNumber)
        {
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DeleteCard";
                    cmd.CommandType = CommandType.StoredProcedure;

                    var parameter = cmd.CreateParameter();
                    parameter.ParameterName = "@cardnumber";
                    parameter.Value = cardNumber;
                    cmd.Parameters.Add(parameter);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateCardBalanceAsync(string cardNumber, float balance)
        {
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UpdateCardBalance";
                    cmd.CommandType = CommandType.StoredProcedure;

                    var paramCardNumber = cmd.CreateParameter();
                    paramCardNumber.ParameterName = "@cnumber";
                    paramCardNumber.Value = cardNumber;
                    cmd.Parameters.Add(paramCardNumber);

                    var paramBalance = cmd.CreateParameter();
                    paramBalance.ParameterName = "@balance";
                    paramBalance.Value = balance;
                    cmd.Parameters.Add(paramBalance);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<float> GetCardBalanceAsync(string cardNumber)
        {
            float cardBalance = -1;
            using (IDbConnection conn = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetBalance";
                    cmd.CommandType = CommandType.StoredProcedure;

                    var parameter = cmd.CreateParameter();
                    parameter.ParameterName = "@cnumber";
                    parameter.Value = cardNumber;
                    cmd.Parameters.Add(parameter);

                    await conn.OpenAsync();

                    using (IDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            cardBalance = (float)reader.GetDouble(reader.GetOrdinal("balance"));
                        }
                    }
                }
            }
            return cardBalance;
        }
    }
}
