﻿using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

namespace ArtAttack.Model
{
    public class DummyProductModel
    {
        private readonly string _connectionString;

        public DummyProductModel(string connectionString)
        {
            this._connectionString = connectionString;
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
            using (SqlConnection conn = new SqlConnection(this._connectionString))
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
            using (SqlConnection conn = new SqlConnection(this._connectionString))
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
        /// Deletes a DummyProduct record by ID.
        /// </summary>
        /// <param name="id"> The product id.</param>
        public async Task DeleteDummyProduct(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
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
        /// <param name="sellerId"> integer.</param>
        /// <returns> string or null.</returns>
        internal async Task<string?> GetSellerNameAsync(int? sellerId)
        {
            using (SqlConnection connection = new SqlConnection(this._connectionString))
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
        /// <param name="productId"> integer.</param>
        /// <returns> string or null.</returns>
        internal async Task<DummyProduct> GetDummyProductByIdAsync(int productId)
        {
            using (SqlConnection connection = new SqlConnection(this._connectionString))
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
    }
}