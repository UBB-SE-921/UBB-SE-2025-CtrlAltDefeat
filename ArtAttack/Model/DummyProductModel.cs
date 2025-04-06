using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using ArtAttack.Shared;

using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("ArtAttack.Tests")]

namespace ArtAttack.Model
{
    public class DummyProductModel
    {
        private readonly string _connectionString;
        private readonly IDatabaseProvider _databaseProvider;
        private readonly IDatabaseCommand _databaseCommand;

        public DummyProductModel(string connectionString)
    : this(connectionString, new SqlDatabaseProvider())
        {
        }

        public DummyProductModel(string connectionString, IDatabaseProvider databaseProvider)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        }


        /// <summary>
        /// Adds a new DummyProduct record using AddDummyProduct Procedure.
        /// </summary
        /// <param name="name"> The name of the product.</param>
        /// <param name="price"> The price of the product.</param>
        /// <param name="sellerId"> The ID of the seller.</param>"
        /// <param name="productType"> The type of the product.</param>"
        /// <param name="startDate"> The start date of the contract for the product.</param>"
        /// <param name="endDate"> The end date of the contract for the product.</param>"
        public async Task AddDummyProductAsync(string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            using (IDbConnection connection = _databaseProvider.CreateConnection(this._connectionString))
            {
                connection.OpenAsync(); 

                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "AddDummyProduct"; 
                    command.CommandType = CommandType.StoredProcedure;

                    IDbDataParameter nameParam = command.CreateParameter();
                    nameParam.ParameterName = "@name";
                    nameParam.Value = name;
                    command.Parameters.Add(nameParam);

                    IDbDataParameter priceParam = command.CreateParameter();
                    priceParam.ParameterName = "@price";
                    priceParam.Value = price;
                    command.Parameters.Add(priceParam);

                    IDbDataParameter sellerIdParam = command.CreateParameter();
                    sellerIdParam.ParameterName = "@SellerID";
                    sellerIdParam.Value = sellerId;
                    command.Parameters.Add(sellerIdParam);

                    IDbDataParameter productTypeParam = command.CreateParameter();
                    productTypeParam.ParameterName = "@productType";
                    productTypeParam.Value = productType;
                    command.Parameters.Add(productTypeParam);

                    IDbDataParameter startDateParam = command.CreateParameter();
                    startDateParam.ParameterName = "@startDate";
                    startDateParam.Value = startDate;
                    command.Parameters.Add(startDateParam);

                    IDbDataParameter endDateParam = command.CreateParameter();
                    endDateParam.ParameterName = "@endDate";
                    endDateParam.Value = endDate;
                    command.Parameters.Add(endDateParam);

                    command.ExecuteNonQueryAsync();
                }

                connection.Close();
            }
        }

        /// <summary>
        /// Updates an existing DummyProduct record using UpdateDummyProduct Procedure.
        /// </summary>
        /// <param name="id">The product id.</param>
        /// <param name="name"> The name of the product.</param>
        /// <param name="price"> The price of the product.</param>
        /// <param name="sellerId"> The ID of the seller.</param>"
        /// <param name="productType"> The type of the product.</param>"
        /// <param name="startDate"> The start date of the contract for the product.</param>"
        /// <param name="endDate"> The end date of the contract for the product.</param>"
        public async Task UpdateDummyProductAsync(int id, string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            using (IDbConnection connection = _databaseProvider.CreateConnection(this._connectionString))
            {
                await connection.OpenAsync();

                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UpdateDummyProduct";
                    command.CommandType = CommandType.StoredProcedure;

                    // Creating parameters explicitly using IDbDataParameter
                    IDbDataParameter idParam = command.CreateParameter();
                    idParam.ParameterName = "@ID";
                    idParam.Value = id;
                    command.Parameters.Add(idParam);

                    IDbDataParameter nameParam = command.CreateParameter();
                    nameParam.ParameterName = "@Name";
                    nameParam.Value = name;
                    command.Parameters.Add(nameParam);

                    IDbDataParameter priceParam = command.CreateParameter();
                    priceParam.ParameterName = "@Price";
                    priceParam.Value = price;
                    command.Parameters.Add(priceParam);

                    IDbDataParameter sellerIdParam = command.CreateParameter();
                    sellerIdParam.ParameterName = "@SellerID";
                    sellerIdParam.Value = sellerId;
                    command.Parameters.Add(sellerIdParam);

                    IDbDataParameter productTypeParam = command.CreateParameter();
                    productTypeParam.ParameterName = "@ProductType";
                    productTypeParam.Value = productType;
                    command.Parameters.Add(productTypeParam);

                    IDbDataParameter startDateParam = command.CreateParameter();
                    startDateParam.ParameterName = "@StartDate";
                    startDateParam.Value = startDate;
                    command.Parameters.Add(startDateParam);

                    IDbDataParameter endDateParam = command.CreateParameter();
                    endDateParam.ParameterName = "@EndDate";
                    endDateParam.Value = endDate;
                    command.Parameters.Add(endDateParam);

                    // Execute the command
                    await command.ExecuteNonQueryAsync();
                }

                connection.Close();
            }
        }


        /// <summary>
        /// Deletes a DummyProduct record by ID.
        /// </summary>
        /// <param name="id"> The product id.</param>
        public async Task DeleteDummyProduct(int id)
        {
            using (IDbConnection connection = _databaseProvider.CreateConnection(this._connectionString))
            {
                await connection.OpenAsync();

                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DeleteDummyProduct";
                    command.CommandType = CommandType.StoredProcedure;

                    // Creating parameter explicitly using IDbDataParameter
                    IDbDataParameter idParam = command.CreateParameter();
                    idParam.ParameterName = "@ID";
                    idParam.Value = id;
                    command.Parameters.Add(idParam);

                    // Execute the command
                    await command.ExecuteNonQueryAsync();
                }

                connection.Close();
            }
        }

        /// <summary>
        /// Retrieves a Seller's name by its ID.
        /// </summary>
        /// <param name="sellerId">Integer representing the seller's ID.</param>
        /// <returns>String or null.</returns>
        public async Task<string?> GetSellerNameAsync(int? sellerId)
        {
            if (sellerId == null)
            {
                throw new ArgumentNullException(nameof(sellerId));
            }

            using (var connection = _databaseProvider.CreateConnection(_connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "GetSellerById";

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@SellerID";
                    parameter.Value = sellerId;
                    command.Parameters.Add(parameter);

                    connection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString(0);
                        }
                    }
                }
            }

            return null;
        }



        /// <summary>
        /// Retrieves a DummyProduct by its ID.
        /// </summary>
        /// <param name="productId">Integer representing the product ID.</param>
        /// <returns>DummyProduct object or null.</returns>
        public async Task<DummyProduct> GetDummyProductByIdAsync(int productId)
        {
            using (IDbConnection connection = _databaseProvider.CreateConnection(this._connectionString))
            {
                await connection.OpenAsync();  // Opening connection asynchronously

                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "GetDummyProductByID";  // Stored Procedure Name
                    command.CommandType = CommandType.StoredProcedure;

                    // Create and add the parameter for the product ID
                    IDbDataParameter productIdParam = command.CreateParameter();
                    productIdParam.ParameterName = "@productID";
                    productIdParam.Value = productId;
                    command.Parameters.Add(productIdParam);

                    // Execute the query and retrieve the result
                    using (IDataReader reader = await command.ExecuteReaderAsync(CancellationToken.None))  // Execute reader asynchronously
                    {
                        if (await reader.ReadAsync())  // Read the data asynchronously
                        {
                            return new DummyProduct
                            {
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Price = (float)reader.GetDouble(2),
                                SellerID = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                                ProductType = reader.GetString(4),
                                StartDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                                EndDate = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6)
                            };
                        }
                    }
                }

                // If no product found, return null
                return null;
            }
        }

    }
}