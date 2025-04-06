using System;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using Microsoft.Data.SqlClient;

namespace ArtAttack.Model
{
    public class DummyWalletModel
    {
        private readonly string connectionString;

        public DummyWalletModel(string connstring)
        {
            connectionString = connstring;
        }

        /// <summary>
        /// Updates the balance of a wallet in the database
        /// </summary>
        /// <param name="walletID">Id of the wallet to be updated</param>
        /// <param name="balance">Amount to update to</param>
        /// <returns></returns>
        public async Task UpdateWalletBalance(int walletID, float balance)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("UpdateWalletBalance", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id", walletID);
                    cmd.Parameters.AddWithValue("@balance", balance);

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
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetWalletBalance", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id", walletID);
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
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
    }
}
