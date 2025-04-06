using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Data;
using System.Linq;
using ArtAttack.Model;
using ArtAttack.Domain;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ArtAttack.Shared;


namespace ArtAtackTests
{
    public class MockDatabase : IDatabaseProvider
    {
        private IDbConnection _mockConnection;

        public IDbConnection CreateConnection(string connectionString)
        {
            return _mockConnection;
        }

        public void SetupMockConnection(IDbConnection mockConnection)
        {
            _mockConnection = mockConnection;
        }

        public void SetupParameterCheck(string paramName, SqlDbType type, object value)
        {
            // Implementation not needed for current tests
        }

        public void VerifyParameterAdded(string paramName, object value)
        {
            // Implementation not needed for current tests
        }

        public void VerifyConnectionOpenAndClose()
        {
            // Implementation not needed for current tests
        }
    }
}
