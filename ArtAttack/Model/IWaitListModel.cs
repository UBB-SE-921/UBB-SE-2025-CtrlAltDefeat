﻿using System;
using System.Collections.Generic;
using ArtAttack.Domain;

namespace ArtAttack.Model
{
    [Obsolete("This interface is deprecated. Please use IWaitListRepository and IWaitListService instead.")]
    public interface IWaitListModel
    {
        void AddUserToWaitlist(int userId, int productWaitListId);
        void RemoveUserFromWaitlist(int userId, int productWaitListId);
        List<UserWaitList> GetUsersInWaitlist(int waitListProductId);
        List<UserWaitList> GetUserWaitlists(int userId);
        int GetWaitlistSize(int productWaitListId);
        bool IsUserInWaitlist(int userId, int productId);
        int GetUserWaitlistPosition(int userId, int productId);
        List<UserWaitList> GetUsersInWaitlistOrdered(int productId);
    }
}