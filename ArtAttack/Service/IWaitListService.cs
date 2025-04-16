using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.Service
{
    public interface IWaitListService
    {
        void AddUserToWaitlist(int userId, int productId);
        void RemoveUserFromWaitlist(int userId, int productId);
        List<UserWaitList> GetUsersInWaitlist(int waitListProductId);
        List<UserWaitList> GetUserWaitlists(int userId);
        int GetWaitlistSize(int productWaitListId);
        bool IsUserInWaitlist(int userId, int productWaitListId);
        int GetUserWaitlistPosition(int userId, int productId);
        void ScheduleRestockAlerts(int productId, DateTime restockDate);
    }
}