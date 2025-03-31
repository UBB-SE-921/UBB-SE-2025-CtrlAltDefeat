using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ArtAttack.Model
{
    public class OrderHistoryModel
    {
        private readonly string _connectionString;

        public OrderHistoryModel(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID)
        {
            List<DummyProduct> products = new List<DummyProduct>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetDummyProductsFromOrderHistory", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrderHistory", orderHistoryID);
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            DummyProduct product = new DummyProduct
                            {

                                ID = reader.GetInt32(reader.GetOrdinal("productID")),
                                Name = reader.GetString(reader.GetOrdinal("name")),
                                Price = (float)reader.GetDouble(reader.GetOrdinal("price")),
                                ProductType = reader.GetString(reader.GetOrdinal("productType")),
                                SellerID = reader["SellerID"] != DBNull.Value
                                ? reader.GetInt32(reader.GetOrdinal("SellerID")): 0,
                                StartDate = reader["startDate"] != DBNull.Value
                                ? reader.GetDateTime(reader.GetOrdinal("startDate")): DateTime.MinValue,  
                                EndDate = reader["endDate"] != DBNull.Value
                                ? reader.GetDateTime(reader.GetOrdinal("endDate")): DateTime.MaxValue
                            };
                            products.Add(product);
                        }
                    }
                }
            }

            return products;
        }
    }
}
