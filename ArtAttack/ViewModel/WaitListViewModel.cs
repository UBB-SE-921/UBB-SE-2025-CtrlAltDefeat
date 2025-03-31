using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace ArtAttack.Services
{
    public class WaitListViewModel : IWaitListViewModel
    {
        private readonly WaitListModel _waitListModel;
        private readonly DummyProductModel _dummyProductModel;

        public WaitListViewModel(string connectionString)
        {
            _waitListModel = new WaitListModel(connectionString);
            _dummyProductModel = new DummyProductModel(connectionString);
        }

        public void AddUserToWaitlist(int userId, int productId)
        {
            _waitListModel.AddUserToWaitlist(userId, productId);
        }

        public void RemoveUserFromWaitlist(int userId, int productId)
        {
            _waitListModel.RemoveUserFromWaitlist(userId, productId);
        }

        public List<UserWaitList> GetUsersInWaitlist(int waitListProductId)
        {
            return _waitListModel.GetUsersInWaitlist(waitListProductId);
        }

        public List<UserWaitList> GetUserWaitlists(int userId)
        {
            return _waitListModel.GetUserWaitlists(userId);
        }

        public int GetWaitlistSize(int productWaitListId)
        {
            return _waitListModel.GetWaitlistSize(productWaitListId);
        }

        public int GetUserWaitlistPosition(int userId, int productId)
        {
            return _waitListModel.GetUserWaitlistPosition(userId, productId);
        }

        public bool IsUserInWaitlist(int userId, int productWaitListId)
        {
            return _waitListModel.IsUserInWaitlist(userId, productWaitListId);
        }

        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            return await _dummyProductModel.GetSellerNameAsync(sellerId);
        }

        public async Task<DummyProduct> GetDummyProductByIdAsync(int productId)
        {
            return await _dummyProductModel.GetDummyProductByIdAsync(productId);
        }
    }
}