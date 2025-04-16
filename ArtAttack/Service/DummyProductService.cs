using System;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Repository;

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
        {
            dummyProductRepository = new DummyProductRepository(connectionString);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyProductService"/> class with a specified repository.
        /// </summary>
        /// <param name="dummyProductRepository">The repository to use.</param>
        public DummyProductService(IDummyProductRepository dummyProductRepository)
        {
            this.dummyProductRepository = dummyProductRepository ?? throw new ArgumentNullException(nameof(dummyProductRepository));
        }

        /// <inheritdoc/>
        public async Task AddDummyProductAsync(string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            // Validate inputs
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
            if (productType == "borrowed" && startDate > endDate)
            {
                throw new ArgumentException("Start date cannot be after end date", nameof(startDate));
            }

            await dummyProductRepository.AddDummyProductAsync(name, price, sellerId, productType, startDate, endDate);
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
            if (productType == "borrowed" && startDate > endDate)
            {
                throw new ArgumentException("Start date cannot be after end date", nameof(startDate));
            }

            await dummyProductRepository.UpdateDummyProductAsync(id, name, price, sellerId, productType, startDate, endDate);
        }

        /// <inheritdoc/>
        public async Task DeleteDummyProductAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Product ID must be positive", nameof(id));
            }

            await dummyProductRepository.DeleteDummyProduct(id);
        }

        /// <inheritdoc/>
        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            return await dummyProductRepository.GetSellerNameAsync(sellerId);
        }

        /// <inheritdoc/>
        public async Task<DummyProduct> GetDummyProductByIdAsync(int productId)
        {
            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be positive", nameof(productId));
            }

            return await dummyProductRepository.GetDummyProductByIdAsync(productId);
        }
    }
}