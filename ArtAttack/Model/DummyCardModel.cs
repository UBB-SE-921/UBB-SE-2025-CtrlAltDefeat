using System;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using Microsoft.Data.SqlClient;

namespace ArtAttack.Model
{
    public class DummyCardModel
    {
        private readonly string connectionString;

        public DummyCardModel(string connstring)
        {
            connectionString = connstring;
        }

        /// <summary>
        /// Deletes a card from the database using the DeleteCard stored procedure
        /// </summary>
        /// <param name="cardNumber">The card number of the card to be deleted</param>
        /// <returns></returns>
        public async Task DeleteCardAsync(string cardNumber)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("DeleteCard", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@cardnumber", cardNumber);

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
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("UpdateCardBalance", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@cnumber", cardNumber);
                    cmd.Parameters.AddWithValue("@balance", balance);

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
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetBalance", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@cnumber", cardNumber);
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
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
