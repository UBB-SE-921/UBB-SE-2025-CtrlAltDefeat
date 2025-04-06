using System;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using Microsoft.Data.SqlClient;

namespace ArtAttack.Model
{
    public class DummyProductModel : IDummyProductModel
    {
        private readonly string connectionString;

        public DummyProductModel(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Adds a new DummyProduct record to the database
        /// </summary>
        /// <param name="name">Name of the product to be added</param>
        /// <param name="price">Price of the product to be added</param>
        /// <param name="sellerId">Id of the product's seller</param>
        /// <param name="productType">Type of the product</param>
        /// <param name="startDate">Beginning date for the product</param>
        /// <param name="endDate">End date for the product</param>
        /// <returns></returns>
        public async Task AddDummyProductAsync(string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("AddDummyProduct", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Price", price);
                    cmd.Parameters.AddWithValue("@SellerID", sellerId);
                    cmd.Parameters.AddWithValue("@ProductType", productType);
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Updates an existing DummyProduct record in the database
        /// </summary>
        /// <param name="id">Id of the Product to be updated</param>
        /// <param name="name">Name to be updated</param>
        /// <param name="price">Price to be updated</param>
        /// <param name="sellerId">Seller Id to be updated</param>
        /// <param name="productType">Product type to be updated</param>
        /// <param name="startDate">Start date to be updated</param>
        /// <param name="endDate">End date to be updated</param>
        /// <returns></returns>
        public async Task UpdateDummyProductAsync(int id, string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("UpdateDummyProduct", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Price", price);
                    cmd.Parameters.AddWithValue("@SellerID", sellerId);
                    cmd.Parameters.AddWithValue("@ProductType", productType);
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Deletes a DummyProduct record from the database
        /// </summary>
        /// <param name="id">Id of the product to be deleted</param>
        /// <returns></returns>
        public async Task DeleteDummyProduct(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("DeleteDummyProduct", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Retrieves a Seler's name by its ID.
        /// </summary>
        /// <param name="sellerId">Seller Id for which to retrieve name</param>
        /// <returns> string or null.</returns>
        internal async Task<string?> GetSellerNameAsync(int? sellerId)
        {
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                using (SqlCommand command = new SqlCommand("GetSellerById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    if (sellerId.HasValue)
                    {
                        command.Parameters.AddWithValue("@SellerID",  (object)sellerId.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@SellerID", DBNull.Value);
                    }

                    await connection.OpenAsync();

                    object result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        return result.ToString();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves a DummyProduct by its ID.
        /// </summary>
        /// <param name="productId">Product Id of the product to be fetched</param>
        /// <returns> string or null.</returns>
        internal async Task<DummyProduct> GetDummyProductByIdAsync(int productId)
        {
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                using (SqlCommand command = new SqlCommand("GetDummyProductByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@productID", productId);

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
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
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves a Seler's name by its ID
        /// </summary>
        /// <param name="sellerId">Id of the seller to be retrieved</param>
        /// <returns></returns>
        Task<string> IDummyProductModel.GetSellerNameAsync(int? sellerId)
        {
            return GetSellerNameAsync(sellerId);
        }

        /// <summary>
        /// Retrieves a DummyProduct by its ID
        /// </summary>
        /// <param name="productId">Id of the product to be retrieved</param>
        /// <returns></returns>
        Task<DummyProduct> IDummyProductModel.GetDummyProductByIdAsync(int productId)
        {
            return GetDummyProductByIdAsync(productId);
        }
    }
}