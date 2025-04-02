using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ArtAttack.Model
{
    public class OrderHistoryModel : IOrderHistoryModel
    {
        private readonly string _connectionString;

        public OrderHistoryModel(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Retrieves a list of DummyProduct objects from the order history.
        /// </summary>
        /// <param name="orderHistoryID">The ID of the order history. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of DummyProduct objects.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public async Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID)
        {
            List<DummyProduct> dummyProducts = new List<DummyProduct>();

            using (SqlConnection sqlConnection = new SqlConnection(_connectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("GetDummyProductsFromOrderHistory", sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@OrderHistory", orderHistoryID);
                    await sqlConnection.OpenAsync();

                    using (SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync())
                    {
                        while (await sqlDataReader.ReadAsync())
                        {
                            DummyProduct dummyProduct = new DummyProduct
                            {

                                ID = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("productID")),
                                Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("name")),
                                Price = (float)sqlDataReader.GetDouble(sqlDataReader.GetOrdinal("price")),
                                ProductType = sqlDataReader.GetString(sqlDataReader.GetOrdinal("productType")),
                                SellerID = sqlDataReader["SellerID"] != DBNull.Value
                                ? sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("SellerID")) : 0,
                                StartDate = sqlDataReader["startDate"] != DBNull.Value
                                ? sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("startDate")) : DateTime.MinValue,
                                EndDate = sqlDataReader["endDate"] != DBNull.Value
                                ? sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("endDate")) : DateTime.MaxValue
                            };
                            dummyProducts.Add(dummyProduct);
                        }
                    }
                }
            }

            return dummyProducts;
        }
    }
}
