﻿using System;
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
        /// Updates an existing DummyProduct record.
        /// </summary>
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
        /// Deletes a DummyProduct record by ID.
        /// </summary>
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

        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            throw new NotImplementedException();
        }

        public async Task<DummyProduct> GetDummyProductByIdAsync(int productId)
        {
            throw new NotImplementedException();
        }
    }
}

