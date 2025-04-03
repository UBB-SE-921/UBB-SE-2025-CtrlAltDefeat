using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.Data.SqlClient;

namespace ArtAttack.Services
{
    public class WaitListViewModel : IWaitListViewModel
    {
        private readonly WaitListModel waitListModel;
        private readonly DummyProductModel dummyProductModel;

        public WaitListViewModel(string connectionString)
        {
            waitListModel = new WaitListModel(connectionString);
            dummyProductModel = new DummyProductModel(connectionString);
        }

        public void AddUserToWaitlist(int userId, int productId)
        {
            waitListModel.AddUserToWaitlist(userId, productId);
        }

        public void RemoveUserFromWaitlist(int userId, int productId)
        {
            waitListModel.RemoveUserFromWaitlist(userId, productId);
        }

        public List<UserWaitList> GetUsersInWaitlist(int waitListProductId)
        {
            return waitListModel.GetUsersInWaitlist(waitListProductId);
        }

        public List<UserWaitList> GetUserWaitlists(int userId)
        {
            return waitListModel.GetUserWaitlists(userId);
        }

        public int GetWaitlistSize(int productWaitListId)
        {
            return waitListModel.GetWaitlistSize(productWaitListId);
        }

        public int GetUserWaitlistPosition(int userId, int productId)
        {
            return waitListModel.GetUserWaitlistPosition(userId, productId);
        }

        public bool IsUserInWaitlist(int userId, int productWaitListId)
        {
            return waitListModel.IsUserInWaitlist(userId, productWaitListId);
        }

        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            return await dummyProductModel.GetSellerNameAsync(sellerId);
        }

        public async Task<DummyProduct> GetDummyProductByIdAsync(int productId)
        {
            return await dummyProductModel.GetDummyProductByIdAsync(productId);
        }
    }
}