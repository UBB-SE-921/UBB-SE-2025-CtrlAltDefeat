using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

namespace ArtAttack.Model
{
    public class DummyCardModel
    {
        private readonly string _connectionString;

        public DummyCardModel(string connstring)
        {
            _connectionString = connstring;
        }

        public async Task AddCardAsync(CardPaymentDetails paymentDetails)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("AddCard", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@cname", paymentDetails.cardholderName);
                    cmd.Parameters.AddWithValue("@cnumber", paymentDetails.cardNumber);
                    cmd.Parameters.AddWithValue("@cvc", paymentDetails.cvc);
                    cmd.Parameters.AddWithValue("@mon", paymentDetails.month);
                    cmd.Parameters.AddWithValue("@yr", paymentDetails.year);
                    cmd.Parameters.AddWithValue("@country", paymentDetails.country);
                    cmd.Parameters.AddWithValue("@balance", paymentDetails.balance);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteCardAsync(string cardNumber)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
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

        public async Task UpdateCardBalanceAsync(string cardNumber, float balance)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
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

        public async Task<float> GetCardBalanceAsync(string cardNumber)
        {
            float cardBalance = -1;
            using (SqlConnection conn = new SqlConnection(_connectionString))
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
