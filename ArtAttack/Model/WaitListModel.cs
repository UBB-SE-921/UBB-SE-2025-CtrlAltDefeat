using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using ArtAttack.Domain;
using ArtAttack.Repository;
using ArtAttack.Service;
using ArtAttack.Shared;

namespace ArtAttack.Model
{
    [Obsolete("This class is deprecated. Please use WaitListRepository and WaitListService instead.")]
    public class WaitListModel : IWaitListModel
    {
        private readonly IWaitListService waitListService;
        private readonly IDummyProductService dummyProductService;
        private readonly IWaitListRepository waitListRepository;

        public WaitListModel(string connectionString)
        {
            waitListService = new WaitListService(connectionString);
            dummyProductService = new DummyProductService(connectionString);
            waitListRepository = new WaitListRepository(connectionString);
        }

        public WaitListModel(string connectionString, IDatabaseProvider databaseProvider)
        {
            waitListRepository = new WaitListRepository(connectionString, databaseProvider);
            var notificationAdapter = new NotificationDataAdapter(connectionString, databaseProvider);
            var dummyProductModel = new DummyProductModel(connectionString, databaseProvider);
            
            waitListService = new WaitListService(waitListRepository, notificationAdapter);
            dummyProductService = new DummyProductService(dummyProductModel);
        }

        public void AddUserToWaitlist(int userId, int productWaitListId)
        {
            waitListService.AddUserToWaitlist(userId, productWaitListId);
        }

        public void RemoveUserFromWaitlist(int userId, int productWaitListId)
        {
            waitListService.RemoveUserFromWaitlist(userId, productWaitListId);
        }

        public List<UserWaitList> GetUsersInWaitlist(int waitListProductId)
        {
            return waitListService.GetUsersInWaitlist(waitListProductId);
        }

        public List<UserWaitList> GetUserWaitlists(int userId)
        {
            return waitListService.GetUserWaitlists(userId);
        }

        public int GetWaitlistSize(int productWaitListId)
        {
            return waitListService.GetWaitlistSize(productWaitListId);
        }

        public bool IsUserInWaitlist(int userId, int productId)
        {
            return waitListService.IsUserInWaitlist(userId, productId);
        }

        public int GetUserWaitlistPosition(int userId, int productId)
        {
            return waitListService.GetUserWaitlistPosition(userId, productId);
        }

        public List<UserWaitList> GetUsersInWaitlistOrdered(int productId)
        {
            return waitListRepository.GetUsersInWaitlistOrdered(productId);
        }
    }
}