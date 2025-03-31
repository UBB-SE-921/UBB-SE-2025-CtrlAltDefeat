using ArtAttack.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtAttack.Services
{
    public interface IWaitListViewModel
    {
        void AddUserToWaitlist(int userId, int productId);
        void RemoveUserFromWaitlist(int userId, int productWaitListId);
        List<UserWaitList> GetUsersInWaitlist(int waitListProductId);
        List<UserWaitList> GetUserWaitlists(int userId);
        int GetWaitlistSize(int productWaitListId);
        bool IsUserInWaitlist(int userId, int productWaitListId);
        Task<string> GetSellerNameAsync(int? sellerId);
        Task<DummyProduct> GetDummyProductByIdAsync(int productId);
        int GetUserWaitlistPosition(int userId, int productId);

        
    }
}