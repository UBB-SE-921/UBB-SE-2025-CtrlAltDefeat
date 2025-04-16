using System;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Shared;

namespace ArtAttack.Repository
{
    /// <summary>
    /// Provides database operations for wallet management.
    /// </summary>
    public class DummyWalletRepository : IDummyWalletRepository
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyWalletRepository"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public DummyWalletRepository(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyWalletRepository"/> class with a specified database provider.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="databaseProvider">The database provider to use.</param>
        public DummyWalletRepository(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        }

        /// <inheritdoc/>
        public async Task<float> GetWalletBalanceAsync(int userId)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "SELECT balance FROM DummyWallet WHERE userID = @UserID";

                    AddParameter(databaseCommand, "@UserID", userId);

                    await databaseConnection.OpenAsync();
                    var result = await databaseCommand.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToSingle(result);
                    }

                    return 0.0f; // Default balance if no wallet exists
                }
            }
        }

        /// <inheritdoc/>
        public async Task UpdateWalletBalance(int userId, float newBalance)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    // First, check if a wallet exists for this user
                    databaseCommand.CommandText = "SELECT COUNT(*) FROM DummyWallet WHERE userID = @UserID";
                    AddParameter(databaseCommand, "@UserID", userId);

                    await databaseConnection.OpenAsync();
                    int count = Convert.ToInt32(await databaseCommand.ExecuteScalarAsync());

                    // Reset command and parameters
                    databaseCommand.Parameters.Clear();

                    if (count > 0)
                    {
                        // Update existing wallet
                        databaseCommand.CommandText = "UPDATE DummyWallet SET balance = @Balance WHERE userID = @UserID";
                    }
                    else
                    {
                        // Insert new wallet
                        databaseCommand.CommandText = "INSERT INTO DummyWallet (userID, balance) VALUES (@UserID, @Balance)";
                    }

                    AddParameter(databaseCommand, "@UserID", userId);
                    AddParameter(databaseCommand, "@Balance", newBalance);

                    await databaseCommand.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Helper method to add a parameter to a command
        /// </summary>
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