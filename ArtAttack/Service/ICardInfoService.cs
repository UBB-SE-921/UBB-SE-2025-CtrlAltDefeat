using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.Service
{
    /// <summary>
    /// Defines operations for managing card information and processing card payments.
    /// </summary>
    internal interface ICardInfoViewService
    {
        /// <summary>
        /// Asynchronously retrieves the dummy products associated with the specified order history ID.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        /// <returns>A task that represents the asynchronous operation which returns a list of <see cref="DummyProduct"/> objects.</returns>
        Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID);

        /// <summary>
        /// Asynchronously retrieves the order summary for the specified order history ID.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        /// <returns>A task that represents the asynchronous operation which returns an <see cref="OrderSummary"/> object.</returns>
        Task<OrderSummary> GetOrderSummaryAsync(int orderHistoryID);

        /// <summary>
        /// Asynchronously processes the card payment.
        /// </summary>
        /// <param name="cardNumber">The card number.</param>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ProcessCardPaymentAsync(string cardNumber, int orderHistoryID);
    }
}
