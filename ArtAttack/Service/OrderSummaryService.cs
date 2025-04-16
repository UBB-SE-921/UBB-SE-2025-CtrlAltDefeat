using System;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Repository;
using ArtAttack.Shared;

namespace ArtAttack.Service
{
    /// <summary>
    /// Service for managing order summary operations.
    /// </summary>
    public class OrderSummaryService : IOrderSummaryService
    {
        private readonly IOrderSummaryRepository orderSummaryRepository;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderSummaryService"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public OrderSummaryService(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderSummaryService"/> class with a specified database provider.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="databaseProvider">The database provider to use.</param>
        public OrderSummaryService(string connectionString, IDatabaseProvider databaseProvider)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (databaseProvider == null)
            {
                throw new ArgumentNullException(nameof(databaseProvider));
            }

            this.orderSummaryRepository = new OrderSummaryRepository(connectionString, databaseProvider);
        }

        /// <inheritdoc/>
        public async Task UpdateOrderSummaryAsync(int id, float subtotal, float warrantyTax, float deliveryFee, float finalTotal,
                                     string fullName, string email, string phoneNumber, string address,
                                     string postalCode, string additionalInfo, string contractDetails)
        {
            // Validate inputs
            if (id <= 0)
            {
                throw new ArgumentException("Order summary ID must be positive", nameof(id));
            }
            
            if (subtotal < 0)
            {
                throw new ArgumentException("Subtotal cannot be negative", nameof(subtotal));
            }
            
            if (warrantyTax < 0)
            {
                throw new ArgumentException("Warranty tax cannot be negative", nameof(warrantyTax));
            }
            
            if (deliveryFee < 0)
            {
                throw new ArgumentException("Delivery fee cannot be negative", nameof(deliveryFee));
            }
            
            if (finalTotal < 0)
            {
                throw new ArgumentException("Final total cannot be negative", nameof(finalTotal));
            }
            
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException("Full name cannot be empty", nameof(fullName));
            }
            
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be empty", nameof(email));
            }
            
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
            }
            
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException("Address cannot be empty", nameof(address));
            }
            
            if (string.IsNullOrWhiteSpace(postalCode))
            {
                throw new ArgumentException("Postal code cannot be empty", nameof(postalCode));
            }
            
            await orderSummaryRepository.UpdateOrderSummaryAsync(id, subtotal, warrantyTax, deliveryFee, finalTotal,
                                              fullName, email, phoneNumber, address,
                                              postalCode, additionalInfo, contractDetails);
        }

        /// <inheritdoc/>
        public async Task<OrderSummary> GetOrderSummaryByIdAsync(int orderSummaryId)
        {
            if (orderSummaryId <= 0)
            {
                throw new ArgumentException("Order summary ID must be positive", nameof(orderSummaryId));
            }

            return await orderSummaryRepository.GetOrderSummaryByIdAsync(orderSummaryId);
        }
    }
} 