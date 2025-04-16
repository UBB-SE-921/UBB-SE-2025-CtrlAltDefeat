using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Service;
using Microsoft.Data.SqlClient;

namespace ArtAttack.ViewModel
{
    /// <summary>
    /// Provides functionality for managing waitlists in the view model layer.
    /// </summary>
    public class WaitListViewModel : IWaitListViewModel
    {
        private readonly IWaitListService waitListService;
        private readonly IDummyProductService dummyProductService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitListViewModel"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public WaitListViewModel(string connectionString)
        {
            waitListService = new WaitListService(connectionString);
            dummyProductService = new DummyProductService(connectionString);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitListViewModel"/> class with specified services.
        /// </summary>
        /// <param name="waitListService">The waitlist service to use.</param>
        /// <param name="dummyProductService">The dummy product service to use.</param>
        public WaitListViewModel(IWaitListService waitListService, IDummyProductService dummyProductService)
        {
            this.waitListService = waitListService;
            this.dummyProductService = dummyProductService;
        }

        /// <summary>
        /// Adds a user to the waitlist for a specific product.
        /// </summary>
        /// <param name="userId">The ID of the user to be added to the waitlist. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public void AddUserToWaitlist(int userId, int productId)
        {
            waitListService.AddUserToWaitlist(userId, productId);
        }

        /// <summary>
        /// Removes a user from the waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user to be removed from the waitlist. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public void RemoveUserFromWaitlist(int userId, int productId)
        {
            waitListService.RemoveUserFromWaitlist(userId, productId);
        }

        /// <summary>
        /// Retrieves all users in a waitlist for a given product.
        /// </summary>
        /// <param name="waitListProductId">The ID of the product waitlist. Must be a positive integer.</param>
        /// <returns>A list of UserWaitList objects representing the users in the waitlist.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public List<UserWaitList> GetUsersInWaitlist(int waitListProductId)
        {
            return waitListService.GetUsersInWaitlist(waitListProductId);
        }

        /// <summary>
        /// Gets all waitlists that a user is part of.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <returns>A list of UserWaitList objects representing the waitlists the user is part of.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public List<UserWaitList> GetUserWaitlists(int userId)
        {
            return waitListService.GetUserWaitlists(userId);
        }

        /// <summary>
        /// Gets the number of users in a product's waitlist.
        /// </summary>
        /// <param name="productWaitListId">The ID of the product waitlist. Must be a positive integer.</param>
        /// <returns>The number of users in the waitlist.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public int GetWaitlistSize(int productWaitListId)
        {
            return waitListService.GetWaitlistSize(productWaitListId);
        }

        /// <summary>
        /// Gets the position of a user in a product's waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>The position of the user in the waitlist, or -1 if the user is not in the waitlist.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public int GetUserWaitlistPosition(int userId, int productId)
        {
            return waitListService.GetUserWaitlistPosition(userId, productId);
        }

        /// <summary>
        /// Checks if a user is in a product's waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <param name="productWaitListId">The ID of the product waitlist. Must be a positive integer.</param>
        /// <returns>True if the user is in the waitlist, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public bool IsUserInWaitlist(int userId, int productWaitListId)
        {
            return waitListService.IsUserInWaitlist(userId, productWaitListId);
        }

        /// <summary>
        /// Gets the name of the seller asynchronously.
        /// </summary>
        /// <param name="sellerId">The ID of the seller. Can be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the name of the seller.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            return await dummyProductService.GetSellerNameAsync(sellerId);
        }

        /// <summary>
        /// Gets a dummy product by its ID asynchronously.
        /// </summary>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the DummyProduct object.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public async Task<DummyProduct> GetDummyProductByIdAsync(int productId)
        {
            return await dummyProductService.GetDummyProductByIdAsync(productId);
        }
    }
}