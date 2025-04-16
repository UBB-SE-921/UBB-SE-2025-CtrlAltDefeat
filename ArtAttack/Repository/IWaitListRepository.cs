using System.Collections.Generic;
using ArtAttack.Domain;

namespace ArtAttack.Model
{
    public interface IWaitListRepository
    {
        void AddUserToWaitlist(int userId, int productWaitListId);
        List<UserWaitList> GetUsersInWaitlist(int waitListProductId);
        List<UserWaitList> GetUsersInWaitlistOrdered(int productId);
        int GetUserWaitlistPosition(int userId, int productId);
        List<UserWaitList> GetUserWaitlists(int userId);
        int GetWaitlistSize(int productWaitListId);
        bool IsUserInWaitlist(int userId, int productId);
        void RemoveUserFromWaitlist(int userId, int productWaitListId);
    }
}