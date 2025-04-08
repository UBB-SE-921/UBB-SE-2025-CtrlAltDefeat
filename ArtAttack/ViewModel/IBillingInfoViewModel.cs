using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.ViewModel
{
    /// <summary>
    /// Defines the methods required for a billing information view model.
    /// </summary>
    public interface IBillingInfoModelView
    {
        /// <summary>
        /// Asynchronously retrieves dummy products associated with the specified order history.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        /// <returns>A task representing the asynchronous operation that returns a list of <see cref="DummyProduct"/> objects.</returns>
        Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID);

        /// <summary>
        /// Calculates the total order amount for the specified order history, including applicable fees.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        void CalculateOrderTotal(int orderHistoryID);

        /// <summary>
        /// Applies the borrowed tax calculation to the specified dummy product.
        /// </summary>
        /// <param name="dummyProduct">The dummy product on which to apply the borrowed tax.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ApplyBorrowedTax(DummyProduct dummyProduct);
    }
}
