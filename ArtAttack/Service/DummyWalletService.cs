using System;
using System.Threading.Tasks;
using ArtAttack.Repository;
using ArtAttack.Shared;

namespace ArtAttack.Service
{
    /// <summary>
    /// Service for managing wallet operations.
    /// </summary>
    public class DummyWalletService : IDummyWalletService
    {
        private readonly IDummyWalletRepository dummyWalletRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyWalletService"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public DummyWalletService(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyWalletService"/> class with a specified database provider.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="databaseProvider">The database provider to use.</param>
        public DummyWalletService(string connectionString, IDatabaseProvider databaseProvider)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (databaseProvider == null)
            {
                throw new ArgumentNullException(nameof(databaseProvider));
            }

            this.dummyWalletRepository = new DummyWalletRepository(connectionString, databaseProvider);
        }

        /// <inheritdoc/>
        public async Task<float> GetWalletBalanceAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be positive", nameof(userId));
            }

            return await dummyWalletRepository.GetWalletBalanceAsync(userId);
        }

        /// <inheritdoc/>
        public async Task UpdateWalletBalance(int userId, float newBalance)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be positive", nameof(userId));
            }

            if (newBalance < 0)
            {
                throw new ArgumentException("Wallet balance cannot be negative", nameof(newBalance));
            }

            await dummyWalletRepository.UpdateWalletBalance(userId, newBalance);
        }
    }
}