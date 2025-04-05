﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtAttack.Model;
using ArtAttack.Domain;
using Microsoft.Data.SqlClient;

[TestClass]
public class ContractRenewalModelTests
{
    private Mock<SqlConnection> mockConnection;
    private ContractRenewalModel model;

    [TestInitialize]
    public void Setup()
    {
        mockConnection = new Mock<SqlConnection>();
        model = new ContractRenewalModel("YourConnectionString");
    }

    [TestMethod]
    public async Task AddRenewedContractAsync_ShouldExecuteNonQuery()
    {
        // Arrange
        var contract = new Contract { OrderID = 1, ContractContent = "Content", RenewalCount = 1, PDFID = 1 };
        byte[] pdfFile = new byte[0];
        var mockCommand = new Mock<SqlCommand>();

        // Act
        await model.AddRenewedContractAsync(contract, pdfFile);

        // Assert
        mockCommand.Verify(c => c.ExecuteNonQueryAsync(), Times.Once);
    }

    [TestMethod]
    public async Task HasContractBeenRenewedAsync_ShouldReturnTrueIfRenewed()
    {
        // Arrange
        long contractId = 1;
        var mockCommand = new Mock<SqlCommand>();
        mockCommand.Setup(c => c.ExecuteScalarAsync()).ReturnsAsync((int?)1); // Updated line

        // Act
        var result = await model.HasContractBeenRenewedAsync(contractId);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task GetRenewedContractsAsync_ShouldReturnContracts()
    {
        // Arrange
        var mockCommand = new Mock<SqlCommand>();
        var mockReader = new Mock<SqlDataReader>();

        // Setup mock data reader
        mockReader.SetupSequence(r => r.Read())
                  .Returns(true)
                  .Returns(false);
        mockReader.Setup(r => r.GetInt32(It.IsAny<int>())).Returns(1);
        mockReader.Setup(r => r.GetString(It.IsAny<int>())).Returns("RENEWED");

        // Setup mock command
        mockCommand.Setup(c => c.ExecuteReaderAsync()).ReturnsAsync(mockReader.Object);

        // Act
        var contracts = await model.GetRenewedContractsAsync();

        // Assert
        Assert.IsNotNull(contracts);
        Assert.AreEqual(1, contracts.Count);
    }
}
