using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Repository;

namespace ArtAttack.Service
{
    public class CardInfoService
    {
        private readonly IOrderHistoryModel orderHistoryModel;
        private readonly IOrderSummaryModel orderSummaryModel;
        private readonly IDummyCardRepository dummyCardRepository;

        public CardInfoService(
            IOrderHistoryModel orderHistoryModel,
            IOrderSummaryModel orderSummaryModel,
            IDummyCardRepository dummyCardRepository)
        {
            this.orderHistoryModel = orderHistoryModel ?? throw new ArgumentNullException(nameof(orderHistoryModel));
            this.orderSummaryModel = orderSummaryModel ?? throw new ArgumentNullException(nameof(orderSummaryModel));
            this.dummyCardRepository = dummyCardRepository ?? throw new ArgumentNullException(nameof(dummyCardRepository));
        }

        /// <summary>
        /// Retrieves dummy products associated with the specified order history.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        /// <returns>A list of <see cref="DummyProduct"/> objects.</returns>
        public async Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID)
        {
            return await orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(orderHistoryID);
        }

        /// <summary>
        /// Retrieves the order summary for the specified order history ID.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        /// <returns>An <see cref="OrderSummary"/> object.</returns>
        public async Task<OrderSummary> GetOrderSummaryAsync(int orderHistoryID)
        {
            return await orderSummaryModel.GetOrderSummaryByIDAsync(orderHistoryID);
        }

        /// <summary>
        /// Processes the card payment by deducting the order total from the card balance.
        /// </summary>
        /// <param name="cardNumber">The card number.</param>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ProcessCardPaymentAsync(string cardNumber, int orderHistoryID)
        {
            float balance = await dummyCardRepository.GetCardBalanceAsync(cardNumber);
            OrderSummary orderSummary = await GetOrderSummaryAsync(orderHistoryID);

            float totalSum = orderSummary.FinalTotal;
            float newBalance = balance - totalSum;

            await dummyCardRepository.UpdateCardBalanceAsync(cardNumber, newBalance);
        }
    }
}
