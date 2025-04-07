using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.ViewModel
{
    /// <summary>
    /// Defines operations for finalizing the purchase process.
    /// </summary>
    internal interface IFinalizePurchaseViewModel
    {
        /// <summary>
        /// Asynchronously retrieves the dummy products associated with the specified order history.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        /// <returns>A task that represents the asynchronous operation and returns a list of <see cref="DummyProduct"/> objects.</returns>
        Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID);
    }
}
