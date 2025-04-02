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

            using (SqlConnection SQLconnection = new SqlConnection(_connectionString))
            {
                using (SqlCommand SQLcommand = new SqlCommand("GetDummyProductsFromOrderHistory", SQLconnection))
                {
                    SQLcommand.CommandType = CommandType.StoredProcedure;
                    SQLcommand.Parameters.AddWithValue("@OrderHistory", orderHistoryID);
                    await SQLconnection.OpenAsync();

                    using (SqlDataReader SQLDataReader = await SQLcommand.ExecuteReaderAsync())
                    {
                        while (await SQLDataReader.ReadAsync())
                        {
                            DummyProduct dummyProduct = new DummyProduct
                            {

                                ID = SQLDataReader.GetInt32(SQLDataReader.GetOrdinal("productID")),
                                Name = SQLDataReader.GetString(SQLDataReader.GetOrdinal("name")),
                                Price = (float)SQLDataReader.GetDouble(SQLDataReader.GetOrdinal("price")),
                                ProductType = SQLDataReader.GetString(SQLDataReader.GetOrdinal("productType")),
                                SellerID = SQLDataReader["SellerID"] != DBNull.Value
                                ? SQLDataReader.GetInt32(SQLDataReader.GetOrdinal("SellerID")) : 0,
                                StartDate = SQLDataReader["startDate"] != DBNull.Value
                                ? SQLDataReader.GetDateTime(SQLDataReader.GetOrdinal("startDate")) : DateTime.MinValue,
                                EndDate = SQLDataReader["endDate"] != DBNull.Value
                                ? SQLDataReader.GetDateTime(SQLDataReader.GetOrdinal("endDate")) : DateTime.MaxValue
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
