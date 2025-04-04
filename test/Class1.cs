﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtAttack;


namespace test
{
    [TestClass]
    public class Program
    {
        [TestMethod]
        public void Main()
        {
            // Arrange
            var expectedConnectionString = "Data Source=RAZVAN\\SQLEXPRESS01;Initial Catalog=PurchaseDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";

            // Act
            var actualConnectionString = ArtAttack.Shared.Configuration._CONNECTION_STRING_;

            // Assert
            Assert.AreEqual(expectedConnectionString, actualConnectionString, "The connection string should match the expected value.");
        }
    }
}
