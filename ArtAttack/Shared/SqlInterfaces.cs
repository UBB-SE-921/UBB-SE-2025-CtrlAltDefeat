using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ArtAttack.Shared
{
    public interface IDatabaseProvider
    {
        IDbConnection CreateConnection(string connectionString);
    }

    public class SqlDatabaseProvider : IDatabaseProvider
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}
