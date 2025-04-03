﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtAttack;


namespace ArtAttack.Tests
{
    [TestClass]
    public class AppTests
    {
        [TestMethod]
        public void ConfigurationConnectionStringTest()
        {
            // Arrange
            var expectedConnectionString = "Data Source=DESKTOP-0NIHISS\\SQLEXPRESS;Initial Catalog=PurchaseDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";

            // Act
            var actualConnectionString = ArtAttack.Shared.Configuration._CONNECTION_STRING_;

            // Assert
            Assert.AreEqual(expectedConnectionString, actualConnectionString, "The connection string should match the expected value.");
        }
    }
}
