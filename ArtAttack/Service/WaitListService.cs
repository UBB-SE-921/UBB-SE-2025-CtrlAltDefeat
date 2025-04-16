using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model;

namespace ArtAttack.Service
{
    internal class WaitListService : IWaitListService
    {
        private readonly IWaitListRepository waitListRepository;
        private readonly IDummyProductRepository dummyProductRepository;

        public WaitListService(IWaitListRepository waitListRepository, IDummyProductRepository dummyProductRepository)
        {
            this.waitListRepository = waitListRepository ?? throw new ArgumentNullException(nameof(waitListRepository));
            this.dummyProductRepository = dummyProductRepository ?? throw new ArgumentNullException(nameof(dummyProductRepository));
        }

        public void AddUserToWaitlist(int userId, int productWaitListId)
        {
            waitListRepository.AddUserToWaitlist(userId, productWaitListId);
        }

        public void RemoveUserFromWaitlist(int userId, int productWaitListId)
        {
            waitListRepository.RemoveUserFromWaitlist(userId, productWaitListId);
        }

        public List<UserWaitList> GetUsersInWaitlist(int waitListProductId)
        {
            return waitListRepository.GetUsersInWaitlist(waitListProductId);
        }

        public List<UserWaitList> GetUserWaitlists(int userId)
        {
            return waitListRepository.GetUserWaitlists(userId);
        }

        public int GetWaitlistSize(int productWaitListId)
        {
            return waitListRepository.GetWaitlistSize(productWaitListId);
        }

        public bool IsUserInWaitlist(int userId, int productId)
        {
            return waitListRepository.IsUserInWaitlist(userId, productId);
        }

        public int GetUserWaitlistPosition(int userId, int productId)
        {
            return waitListRepository.GetUserWaitlistPosition(userId, productId);
        }

        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            return await dummyProductRepository.GetSellerNameAsync(sellerId);
        }

        public async Task<DummyProduct> GetDummyProductByIdAsync(int productId)
        {
            return await dummyProductRepository.GetDummyProductByIdAsync(productId);
        }
    }
}
