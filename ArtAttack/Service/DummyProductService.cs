using System;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Repository;
using ArtAttack.Shared;

namespace ArtAttack.Service
{
    /// <summary>
    /// Service for managing dummy product operations.
    /// </summary>
    public class DummyProductService : IDummyProductService
    {
        private readonly IDummyProductRepository dummyProductRepository;
        /// <summary>
        /// Initializes a new instance of the <see cref="DummyProductService"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public DummyProductService(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyProductService"/> class with a specified database provider.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="databaseProvider">The database provider to use.</param>
        public DummyProductService(string connectionString, IDatabaseProvider databaseProvider)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (databaseProvider == null)
            {
                throw new ArgumentNullException(nameof(databaseProvider));
            }

            this.dummyProductRepository = new DummyProductRepository(connectionString, databaseProvider);
        }

        /// <inheritdoc/>
        public async Task UpdateDummyProductAsync(int id, string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            // Validate inputs
            if (id <= 0)
            {
                throw new ArgumentException("Product ID must be positive", nameof(id));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Product name cannot be empty", nameof(name));
            }
            if (price < 0)
            {
                throw new ArgumentException("Price cannot be negative", nameof(price));
            }

            if (sellerId < 0)
            {
                throw new ArgumentException("Seller ID cannot be negative", nameof(sellerId));
            }
            if (string.IsNullOrWhiteSpace(productType))
            {
                throw new ArgumentException("Product type cannot be empty", nameof(productType));
            }

            // Only validate start and end dates for borrowed products
            if (productType == "borrowed")
            {
                if (startDate > endDate)
                {
                    throw new ArgumentException("Start date cannot be after end date", nameof(startDate));
                }
            }
            await dummyProductRepository.UpdateDummyProductAsync(id, name, price, sellerId, productType, startDate, endDate);
        }
    }
}