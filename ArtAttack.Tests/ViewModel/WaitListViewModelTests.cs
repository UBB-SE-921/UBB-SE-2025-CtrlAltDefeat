using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Repository;
using ArtAttack.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtAttack.Tests.ViewModel
{
    [TestClass]
    public class WaitListViewModelTests
    {
        private Mock<IWaitListRepository> mockWaitListModel;
        private Mock<IDummyProductRepository> mockDummyProductModel;
        private WaitListRepository waitListViewModel;
        private string testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";

        [TestInitialize]
        public void InitializeTestDependencies()
        {
            mockWaitListModel = new Mock<IWaitListRepository>();
            mockDummyProductModel = new Mock<IDummyProductRepository>();

            var viewModel = new WaitListRepository(testConnectionString);

            var waitListModelField = typeof(WaitListRepository).GetField("waitListModel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var dummyProductModelField = typeof(WaitListRepository).GetField("dummyProductModel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            waitListModelField.SetValue(viewModel, mockWaitListModel.Object);
            dummyProductModelField.SetValue(viewModel, mockDummyProductModel.Object);

            waitListViewModel = viewModel;
        }

        [TestMethod]
        public void AddUserToWaitlist_ShouldCallModelWithCorrectParameters()
        {
            int userId = 42;
            int productId = 101;

            waitListViewModel.AddUserToWaitlist(userId, productId);

            mockWaitListModel.Verify(model => model.AddUserToWaitlist(userId, productId), Times.Once);
        }

        [TestMethod]
        public void RemoveUserFromWaitlist_ShouldCallModelWithCorrectParameters()
        {
            int userId = 42;
            int productId = 101;

            waitListViewModel.RemoveUserFromWaitlist(userId, productId);

            mockWaitListModel.Verify(model => model.RemoveUserFromWaitlist(userId, productId), Times.Once);
        }

        [TestMethod]
        public void GetUsersInWaitlist_ShouldReturnCorrectUserList()
        {
            int productWaitListId = 101;
            var expectedUsers = new List<UserWaitList>
            {
                new UserWaitList { UserID = 1, ProductWaitListID = productWaitListId, PositionInQueue = 1 },
                new UserWaitList { UserID = 2, ProductWaitListID = productWaitListId, PositionInQueue = 2 }
            };

            mockWaitListModel.Setup(model => model.GetUsersInWaitlist(productWaitListId))
                .Returns(expectedUsers);

            var result = waitListViewModel.GetUsersInWaitlist(productWaitListId);

            Assert.AreEqual(expectedUsers.Count, result.Count);
        }

        [TestMethod]
        public void GetUserWaitlists_ShouldReturnCorrectWaitlists()
        {
            int userId = 42;
            var expectedWaitlists = new List<UserWaitList>
            {
                new UserWaitList { UserID = userId, ProductWaitListID = 101, PositionInQueue = 1 },
                new UserWaitList { UserID = userId, ProductWaitListID = 102, PositionInQueue = 3 }
            };

            mockWaitListModel.Setup(model => model.GetUserWaitlists(userId))
                .Returns(expectedWaitlists);

            var result = waitListViewModel.GetUserWaitlists(userId);

            Assert.AreEqual(expectedWaitlists.Count, result.Count);
        }

        [TestMethod]
        public void GetWaitlistSize_ShouldReturnCorrectSize()
        {
            int productWaitListId = 101;
            int expectedSize = 5;

            mockWaitListModel.Setup(model => model.GetWaitlistSize(productWaitListId))
                .Returns(expectedSize);

            var result = waitListViewModel.GetWaitlistSize(productWaitListId);

            Assert.AreEqual(expectedSize, result);
        }

        [TestMethod]
        public void GetUserWaitlistPosition_ShouldReturnCorrectPosition()
        {
            int userId = 42;
            int productId = 101;
            int expectedPosition = 3;

            mockWaitListModel.Setup(model => model.GetUserWaitlistPosition(userId, productId))
                .Returns(expectedPosition);

            var result = waitListViewModel.GetUserWaitlistPosition(userId, productId);

            Assert.AreEqual(expectedPosition, result);
        }

        [TestMethod]
        public void IsUserInWaitlist_ShouldReturnCorrectResult()
        {
            int userId = 42;
            int productWaitListId = 101;
            bool expectedResult = true;

            mockWaitListModel.Setup(model => model.IsUserInWaitlist(userId, productWaitListId))
                .Returns(expectedResult);

            var result = waitListViewModel.IsUserInWaitlist(userId, productWaitListId);

            Assert.AreEqual(expectedResult, result);
        }
    }
}
