using System;
using System.Diagnostics.CodeAnalysis;
using ArtAttack.Service;

namespace ArtAttack.ViewModel
{
    [ExcludeFromCodeCoverage]
    public class WaitListNotifier
    {
        private readonly IWaitListService waitListService;

        public WaitListNotifier(string connectionString)
        {
            waitListService = new WaitListService(connectionString);
        }

        public WaitListNotifier(IWaitListService waitListService)
        {
            this.waitListService = waitListService;
        }

        /// <summary>
        /// Schedules restock alerts for users on the waitlist for a specific product.
        /// </summary>
        /// <param name="productId">The ID of the product to restock. Must be a positive integer.</param>
        /// <param name="restockDate">The date and time when the product will be restocked.</param>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <precondition>productId must be a valid product ID. restockDate must be a future date.</precondition>
        /// <postcondition>Notifications are scheduled for users on the waitlist.</postcondition>
        public void ScheduleRestockAlerts(int productId, DateTime restockDate)
        {
            waitListService.ScheduleRestockAlerts(productId, restockDate);
        }
    }
}