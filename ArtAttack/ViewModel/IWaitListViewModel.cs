using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.ViewModel
{
    /// <summary>
    /// Defines operations for managing waitlist functionality for users and products.
    /// </summary>
    public interface IWaitListViewModel
    {
        /// <summary>
        /// Adds a user to the waitlist for a specified product.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to add.</param>
        /// <param name="productId">The unique identifier of the product.</param>
        void AddUserToWaitlist(int userId, int productId);

        /// <summary>
        /// Removes a user from the waitlist for a specified product.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to remove.</param>
        /// <param name="productId">The unique identifier of the product.</param>
        void RemoveUserFromWaitlist(int userId, int productId);

        /// <summary>
        /// Retrieves all users in the waitlist for the specified product.
        /// </summary>
        /// <param name="waitListProductId">The unique identifier of the product waitlist.</param>
        /// <returns>
        /// A list of <see cref="UserWaitList"/> objects representing the users in the waitlist.
        /// </returns>
        List<UserWaitList> GetUsersInWaitlist(int waitListProductId);

        /// <summary>
        /// Retrieves all waitlists that a specific user is part of.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>
        /// A list of <see cref="UserWaitList"/> objects representing the waitlists the user is participating in.
        /// </returns>
        List<UserWaitList> GetUserWaitlists(int userId);

        /// <summary>
        /// Retrieves the size of the waitlist for the specified product.
        /// </summary>
        /// <param name="productWaitListId">The unique identifier of the product waitlist.</param>
        /// <returns>The size of the waitlist.</returns>
        int GetWaitlistSize(int productWaitListId);

        /// <summary>
        /// Determines whether a user is already present in the waitlist for a specified product.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <returns><c>true</c> if the user is in the waitlist; otherwise, <c>false</c>.</returns>
        bool IsUserInWaitlist(int userId, int productId);

        /// <summary>
        /// Retrieves the waitlist position of a user for a specific product.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <returns>The user's position in the waitlist.</returns>
        int GetUserWaitlistPosition(int userId, int productId);

        /// <summary>
        /// Asynchronously retrieves the seller's name based on an optional seller identifier.
        /// </summary>
        /// <param name="sellerId">The unique identifier of the seller (optional).</param>
        /// <returns>A task that returns the seller's name as a string.</returns>
        Task<string> GetSellerNameAsync(int? sellerId);

        /// <summary>
        /// Asynchronously retrieves the dummy product corresponding to the specified product ID.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <returns>A task that returns the <see cref="DummyProduct"/> object.</returns>
        Task<DummyProduct> GetDummyProductByIdAsync(int productId);
    }
}
