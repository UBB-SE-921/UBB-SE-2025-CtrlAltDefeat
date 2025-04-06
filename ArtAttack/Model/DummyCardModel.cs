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

        /// <summary>
        /// Deletes a card from the database using the DeleteCard stored procedure
        /// </summary>
        /// <param name="cardNumber">The card number of the card to be deleted</param>
        /// <returns></returns>
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

        /// <summary>
        /// Updates the balance of a card in the database using the UpdateCardBalance stored procedure
        /// </summary>
        /// <param name="cardNumber">The number of the card to be updated</param>
        /// <param name="balance">The balance amount the card to be updated to</param>
        /// <returns></returns>
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

        /// <summary>
        /// Retrieves the balance of a card from the database using the GetBalance stored procedure
        /// </summary>
        /// <param name="cardNumber">The number of the card of which to get the balance from</param>
        /// <returns></returns>
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
