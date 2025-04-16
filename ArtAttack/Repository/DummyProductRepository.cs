using System;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Repository
{
    /// <summary>
    /// Provides database operations for dummy product management.
    /// </summary>
    public class DummyProductRepository : IDummyProductRepository
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyProductRepository"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public DummyProductRepository(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyProductRepository"/> class with a specified database provider.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="databaseProvider">The database provider to use.</param>
        public DummyProductRepository(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        }

        /// <inheritdoc/>
        public async Task UpdateDummyProductAsync(int id, string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "UPDATE DummyProduct SET name = @Name, price = @Price, SellerID = @SellerID, productType = @ProductType, startDate = @StartDate, endDate = @EndDate WHERE ID = @ID";

                    AddParameter(databaseCommand, "@ID", id);
                    AddParameter(databaseCommand, "@Name", name);
                    AddParameter(databaseCommand, "@Price", price);
                    AddParameter(databaseCommand, "@SellerID", sellerId);
                    AddParameter(databaseCommand, "@ProductType", productType);

                    if (productType == "borrowed")
                    {
                        AddParameter(databaseCommand, "@StartDate", startDate);
                        AddParameter(databaseCommand, "@EndDate", endDate);
                    }
                    else
                    {
                        AddParameter(databaseCommand, "@StartDate", DBNull.Value);
                        AddParameter(databaseCommand, "@EndDate", DBNull.Value);
                    }

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Helper method to add a parameter to a command
        /// </summary>
        private void AddParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;

            if (value == null || value == DBNull.Value)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.Value = value;
            }

            command.Parameters.Add(parameter);
        }
    }
}