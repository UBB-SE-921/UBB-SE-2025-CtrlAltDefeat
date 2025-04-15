using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Repository
{
    public class DummyCardRepository : IDummyCardRepository
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        [ExcludeFromCodeCoverage]
        public DummyCardRepository(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        public DummyCardRepository(string connectionString, IDatabaseProvider databaseProvider)
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
        /// Deletes a card from the database using the DeleteCard stored procedure
        /// </summary>
        /// <param name="cardNumber">The card number of the card to be deleted</param>
        /// <returns></returns>
        public async Task DeleteCardAsync(string cardNumber)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "DeleteCard";
                    databaseCommand.CommandType = CommandType.StoredProcedure;

                    var parameter = databaseCommand.CreateParameter();
                    parameter.ParameterName = "@cardnumber";
                    parameter.Value = cardNumber;
                    databaseCommand.Parameters.Add(parameter);

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
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
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "UpdateCardBalance";
                    databaseCommand.CommandType = CommandType.StoredProcedure;

                    var paramCardNumber = databaseCommand.CreateParameter();
                    paramCardNumber.ParameterName = "@cnumber";
                    paramCardNumber.Value = cardNumber;
                    databaseCommand.Parameters.Add(paramCardNumber);

                    var paramBalance = databaseCommand.CreateParameter();
                    paramBalance.ParameterName = "@balance";
                    paramBalance.Value = balance;
                    databaseCommand.Parameters.Add(paramBalance);

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
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
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "GetBalance";
                    databaseCommand.CommandType = CommandType.StoredProcedure;

                    var parameter = databaseCommand.CreateParameter();
                    parameter.ParameterName = "@cnumber";
                    parameter.Value = cardNumber;
                    databaseCommand.Parameters.Add(parameter);

                    await databaseConnection.OpenAsync();

                    using (IDataReader reader = await databaseCommand.ExecuteReaderAsync())
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
