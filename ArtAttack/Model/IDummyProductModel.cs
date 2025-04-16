using System;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.Model
{
    [Obsolete("This interface is deprecated. Please use IDummyProductRepository and IDummyProductService instead.")]
    public interface IDummyProductModel
    {
        Task AddDummyProductAsync(string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate);
        Task UpdateDummyProductAsync(int id, string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate);
        Task DeleteDummyProduct(int id);
        Task<string> GetSellerNameAsync(int? sellerId);
        Task<DummyProduct> GetDummyProductByIdAsync(int productId);
    }
}