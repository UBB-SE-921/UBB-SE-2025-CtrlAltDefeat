using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Repository;
using ArtAttack.Shared;

namespace ArtAttack.Service
{
    /// <summary>
    /// Service for managing order history operations.
    /// </summary>
    public class OrderHistoryService : IOrderHistoryService
    {
        private readonly IOrderHistoryRepository orderHistoryRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryService"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public OrderHistoryService(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryService"/> class with a specified database provider.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="databaseProvider">The database provider to use.</param>
        public OrderHistoryService(string connectionString, IDatabaseProvider databaseProvider)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (databaseProvider == null)
            {
                throw new ArgumentNullException(nameof(databaseProvider));
            }

            this.orderHistoryRepository = new OrderHistoryRepository(connectionString, databaseProvider);
        }

        /// <inheritdoc/>
        public async Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryId)
        {
            if (orderHistoryId <= 0)
            {
                throw new ArgumentException("Order history ID must be positive", nameof(orderHistoryId));
            }
            
            return await orderHistoryRepository.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);
        }
    }
} 