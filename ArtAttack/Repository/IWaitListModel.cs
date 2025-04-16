using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.Repository
{
    public interface IWaitListRepository
    {
        void AddUserToWaitlist(int userId, int productWaitListId);
        void RemoveUserFromWaitlist(int userId, int productWaitListId);
        List<UserWaitList> GetUsersInWaitlist(int waitListProductId);
        List<UserWaitList> GetUserWaitlists(int userId);
        int GetWaitlistSize(int productWaitListId);
        bool IsUserInWaitlist(int userId, int productId);
        int GetUserWaitlistPosition(int userId, int productId);
        List<UserWaitList> GetUsersInWaitlistOrdered(int productId);
        int GetWaitlistProductId(int productId);
    }
}
