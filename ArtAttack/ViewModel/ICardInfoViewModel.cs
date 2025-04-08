﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.ViewModel
{
    /// <summary>
    /// Defines operations for managing card information and processing card payments.
    /// </summary>
    internal interface ICardInfoViewModel
    {
        /// <summary>
        /// Asynchronously retrieves the dummy products associated with the specified order history ID.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        /// <returns>A task that represents the asynchronous operation which returns a list of <see cref="DummyProduct"/> objects.</returns>
        Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID);

        /// <summary>
        /// Asynchronously processes the card payment.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ProcessCardPaymentAsync();
    }
}
