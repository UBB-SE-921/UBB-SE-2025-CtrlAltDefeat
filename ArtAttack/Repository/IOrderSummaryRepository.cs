using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.Repository
{
    /// <summary>
    /// Defines database operations for order summaries.
    /// </summary>
    public interface IOrderSummaryRepository
    {
        /// <summary>
        /// Updates an order summary in the database.
        /// </summary>
        /// <param name="id">The ID of the order summary to update.</param>
        /// <param name="subtotal">The subtotal of the order.</param>
        /// <param name="warrantyTax">The warranty tax of the order.</param>
        /// <param name="deliveryFee">The delivery fee of the order.</param>
        /// <param name="finalTotal">The final total of the order.</param>
        /// <param name="fullName">The customer's full name.</param>
        /// <param name="email">The customer's email.</param>
        /// <param name="phoneNumber">The customer's phone number.</param>
        /// <param name="address">The customer's address.</param>
        /// <param name="postalCode">The customer's postal code.</param>
        /// <param name="additionalInfo">Additional information about the order.</param>
        /// <param name="contractDetails">Contract details for borrowed items.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateOrderSummaryAsync(int id, float subtotal, float warrantyTax, float deliveryFee, float finalTotal,
                                     string fullName, string email, string phoneNumber, string address,
                                     string postalCode, string additionalInfo, string contractDetails);
        
        /// <summary>
        /// Retrieves an order summary by its ID.
        /// </summary>
        /// <param name="orderSummaryId">The ID of the order summary to retrieve.</param>
        /// <returns>A task representing the asynchronous operation that returns the order summary.</returns>
        Task<OrderSummary> GetOrderSummaryByIdAsync(int orderSummaryId);
    }
} 