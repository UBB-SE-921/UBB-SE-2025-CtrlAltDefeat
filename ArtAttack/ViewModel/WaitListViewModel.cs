using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.Data.SqlClient;
using ArtAttack.Repository;

namespace ArtAttack.Services
{
    public class WaitListViewModel : IWaitListViewModel
    {
        private readonly IWaitListRepository waitListRepository;
        private readonly IDummyProductRepository dummyProductRepository;

        /// <summary>
        /// Default constructor for WaitListViewModel.
        /// </summary>
        /// <param name="connectionString">The database connection string. Cannot be null or empty.</param>
        /// <remarks>
        /// Initializes a new instance of the WaitListViewModel class with the specified connection string,
        /// creating new instances of the required model dependencies (waitListRepository and dummyProductRepository).
        /// This constructor is typically used in production scenarios where real database connections are needed.
        /// </remarks>
        public WaitListViewModel(string connectionString)
        {
            waitListRepository = new WaitListRepository(connectionString);
            dummyProductRepository = new dummyProductRepository(connectionString);
        }

        /// <summary>
        /// Adds a user to the waitlist for a specific product.
        /// </summary>
        /// <param name="userId">The ID of the user to be added to the waitlist. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public void AddUserToWaitlist(int userId, int productId)
        {
            waitListRepository.AddUserToWaitlist(userId, productId);
        }

        /// <summary>
        /// Removes a user from the waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user to be removed from the waitlist. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public void RemoveUserFromWaitlist(int userId, int productId)
        {
            waitListRepository.RemoveUserFromWaitlist(userId, productId);
        }

        /// <summary>
        /// Retrieves all users in a waitlist for a given product.
        /// </summary>
        /// <param name="waitListProductId">The ID of the product waitlist. Must be a positive integer.</param>
        /// <returns>A list of UserWaitList objects representing the users in the waitlist.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public List<UserWaitList> GetUsersInWaitlist(int waitListProductId)
        {
            return waitListRepository.GetUsersInWaitlist(waitListProductId);
        }

        /// <summary>
        /// Gets all waitlists that a user is part of.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <returns>A list of UserWaitList objects representing the waitlists the user is part of.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public List<UserWaitList> GetUserWaitlists(int userId)
        {
            return waitListRepository.GetUserWaitlists(userId);
        }

        /// <summary>
        /// Gets the number of users in a product's waitlist.
        /// </summary>
        /// <param name="productWaitListId">The ID of the product waitlist. Must be a positive integer.</param>
        /// <returns>The number of users in the waitlist.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public int GetWaitlistSize(int productWaitListId)
        {
            return waitListRepository.GetWaitlistSize(productWaitListId);
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
            return waitListRepository.GetUserWaitlistPosition(userId, productId);
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
            return waitListRepository.IsUserInWaitlist(userId, productWaitListId);
        }

        /// <summary>
        /// Gets the name of the seller asynchronously.
        /// </summary>
        /// <param name="sellerId">The ID of the seller. Can be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the name of the seller.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            return await dummyProductRepository.GetSellerNameAsync(sellerId);
        }

        /// <summary>
        /// Gets a dummy product by its ID asynchronously.
        /// </summary>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the DummyProduct object.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public async Task<DummyProduct> GetDummyProductByIdAsync(int productId)
        {
            return await dummyProductRepository.GetDummyProductByIdAsync(productId);
        }
    }
}