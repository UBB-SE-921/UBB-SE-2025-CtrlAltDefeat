using System;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.Service
{
    /// <summary>
    /// Defines operations for managing dummy products.
    /// </summary>
    public interface IDummyProductService
    {
        /// <summary>
        /// Adds a new dummy product.
        /// </summary>
        /// <param name="name">The name of the product.</param>
        /// <param name="price">The price of the product.</param>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="productType">The type of the product.</param>
        /// <param name="startDate">The start date for borrowed products.</param>
        /// <param name="endDate">The end date for borrowed products.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddDummyProductAsync(string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Updates a dummy product.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="name">The name of the product.</param>
        /// <param name="price">The price of the product.</param>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="productType">The type of the product.</param>
        /// <param name="startDate">The start date for borrowed products.</param>
        /// <param name="endDate">The end date for borrowed products.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateDummyProductAsync(int id, string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Deletes a dummy product.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteDummyProductAsync(int id);
        
        /// <summary>
        /// Gets the name of a seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>The name of the seller.</returns>
        Task<string> GetSellerNameAsync(int? sellerId);
        
        /// <summary>
        /// Gets a dummy product by its ID.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The dummy product, or null if not found.</returns>
        Task<DummyProduct> GetDummyProductByIdAsync(int productId);
    }
} 