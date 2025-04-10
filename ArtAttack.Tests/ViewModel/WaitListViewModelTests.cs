using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Services;
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
        private Mock<IWaitListModel> _mockWaitListModel;
        private Mock<IDummyProductModel> _mockDummyProductModel;
        private WaitListViewModel _waitListViewModel;
        private string _testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";

        [TestInitialize]
        public void Setup()
        {
            _mockWaitListModel = new Mock<IWaitListModel>();
            _mockDummyProductModel = new Mock<IDummyProductModel>();

            // Create a new instance of WaitListViewModel
            var waitListViewModel = new WaitListViewModel(_testConnectionString);

            // Get private fields via reflection - note the field names don't have underscores
            var waitListModelField = typeof(WaitListViewModel).GetField("waitListModel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var dummyProductModelField = typeof(WaitListViewModel).GetField("dummyProductModel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Set our mocks into the private fields
            waitListModelField.SetValue(waitListViewModel, _mockWaitListModel.Object);
            dummyProductModelField.SetValue(waitListViewModel, _mockDummyProductModel.Object);

            _waitListViewModel = waitListViewModel;
        }

        [TestMethod]
        public void AddUserToWaitlist_CallsModelWithCorrectParameters()
        {
            // Arrange
            int userId = 42;
            int productId = 101;

            // Act
            _waitListViewModel.AddUserToWaitlist(userId, productId);

            // Assert
            _mockWaitListModel.Verify(m => m.AddUserToWaitlist(userId, productId), Times.Once);
        }

        [TestMethod]
        public void RemoveUserFromWaitlist_CallsModelWithCorrectParameters()
        {
            // Arrange
            int userId = 42;
            int productId = 101;

            // Act
            _waitListViewModel.RemoveUserFromWaitlist(userId, productId);

            // Assert
            _mockWaitListModel.Verify(m => m.RemoveUserFromWaitlist(userId, productId), Times.Once);
        }

        [TestMethod]
        public void GetUsersInWaitlist_CallsModelAndReturnsCorrectData()
        {
            // Arrange
            int waitListProductId = 101;
            var expectedUsers = new List<UserWaitList>
            {
                new UserWaitList { UserID = 1, ProductWaitListID = waitListProductId, PositionInQueue = 1 },
                new UserWaitList { UserID = 2, ProductWaitListID = waitListProductId, PositionInQueue = 2 }
            };

            _mockWaitListModel.Setup(m => m.GetUsersInWaitlist(waitListProductId))
                .Returns(expectedUsers);

            // Act
            var result = _waitListViewModel.GetUsersInWaitlist(waitListProductId);

            // Assert
            Assert.AreEqual(expectedUsers.Count, result.Count);
            for (int i = 0; i < expectedUsers.Count; i++)
            {
                Assert.AreEqual(expectedUsers[i].UserID, result[i].UserID);
                Assert.AreEqual(expectedUsers[i].ProductWaitListID, result[i].ProductWaitListID);
                Assert.AreEqual(expectedUsers[i].PositionInQueue, result[i].PositionInQueue);
            }
            _mockWaitListModel.Verify(m => m.GetUsersInWaitlist(waitListProductId), Times.Once);
        }

        [TestMethod]
        public void GetUserWaitlists_CallsModelAndReturnsCorrectData()
        {
            // Arrange
            int userId = 42;
            var expectedWaitlists = new List<UserWaitList>
            {
                new UserWaitList { UserID = userId, ProductWaitListID = 101, PositionInQueue = 1 },
                new UserWaitList { UserID = userId, ProductWaitListID = 102, PositionInQueue = 3 }
            };

            _mockWaitListModel.Setup(m => m.GetUserWaitlists(userId))
                .Returns(expectedWaitlists);

            // Act
            var result = _waitListViewModel.GetUserWaitlists(userId);

            // Assert
            Assert.AreEqual(expectedWaitlists.Count, result.Count);
            for (int i = 0; i < expectedWaitlists.Count; i++)
            {
                Assert.AreEqual(expectedWaitlists[i].UserID, result[i].UserID);
                Assert.AreEqual(expectedWaitlists[i].ProductWaitListID, result[i].ProductWaitListID);
                Assert.AreEqual(expectedWaitlists[i].PositionInQueue, result[i].PositionInQueue);
            }
            _mockWaitListModel.Verify(m => m.GetUserWaitlists(userId), Times.Once);
        }

        [TestMethod]
        public void GetWaitlistSize_CallsModelAndReturnsCorrectSize()
        {
            // Arrange
            int productWaitListId = 101;
            int expectedSize = 5;

            _mockWaitListModel.Setup(m => m.GetWaitlistSize(productWaitListId))
                .Returns(expectedSize);

            // Act
            var result = _waitListViewModel.GetWaitlistSize(productWaitListId);

            // Assert
            Assert.AreEqual(expectedSize, result);
            _mockWaitListModel.Verify(m => m.GetWaitlistSize(productWaitListId), Times.Once);
        }

        [TestMethod]
        public void GetUserWaitlistPosition_CallsModelAndReturnsCorrectPosition()
        {
            // Arrange
            int userId = 42;
            int productId = 101;
            int expectedPosition = 3;

            _mockWaitListModel.Setup(m => m.GetUserWaitlistPosition(userId, productId))
                .Returns(expectedPosition);

            // Act
            var result = _waitListViewModel.GetUserWaitlistPosition(userId, productId);

            // Assert
            Assert.AreEqual(expectedPosition, result);
            _mockWaitListModel.Verify(m => m.GetUserWaitlistPosition(userId, productId), Times.Once);
        }

        [TestMethod]
        public void IsUserInWaitlist_CallsModelAndReturnsCorrectResult()
        {
            // Arrange
            int userId = 42;
            int productWaitListId = 101;
            bool expectedResult = true;

            _mockWaitListModel.Setup(m => m.IsUserInWaitlist(userId, productWaitListId))
                .Returns(expectedResult);

            // Act
            var result = _waitListViewModel.IsUserInWaitlist(userId, productWaitListId);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _mockWaitListModel.Verify(m => m.IsUserInWaitlist(userId, productWaitListId), Times.Once);
        }

        [TestMethod]
        public async Task GetSellerNameAsync_CallsModelAndReturnsCorrectName()
        {
            // Arrange
            int? sellerId = 501;
            string expectedName = "Test Seller";

            _mockDummyProductModel.Setup(m => m.GetSellerNameAsync(sellerId))
                .ReturnsAsync(expectedName);

            // Act
            var result = await _waitListViewModel.GetSellerNameAsync(sellerId);

            // Assert
            Assert.AreEqual(expectedName, result);
            _mockDummyProductModel.Verify(m => m.GetSellerNameAsync(sellerId), Times.Once);
        }

        [TestMethod]
        public async Task GetDummyProductByIdAsync_CallsModelAndReturnsCorrectProduct()
        {
            // Arrange
            int productId = 101;
            var expectedProduct = new DummyProduct
            {
                ID = productId,
                Name = "Test Product",
                Price = 19.99f,
                ProductType = "Test Type",
                SellerID = 501
            };

            _mockDummyProductModel.Setup(m => m.GetDummyProductByIdAsync(productId))
                .ReturnsAsync(expectedProduct);

            // Act
            var result = await _waitListViewModel.GetDummyProductByIdAsync(productId);

            // Assert
            Assert.AreEqual(expectedProduct.ID, result.ID);
            Assert.AreEqual(expectedProduct.Name, result.Name);
            Assert.AreEqual(expectedProduct.Price, result.Price);
            Assert.AreEqual(expectedProduct.ProductType, result.ProductType);
            Assert.AreEqual(expectedProduct.SellerID, result.SellerID);
            _mockDummyProductModel.Verify(m => m.GetDummyProductByIdAsync(productId), Times.Once);
        }
    }
}