using ArtAttack.Domain;
using System;
using System.Threading.Tasks;

namespace ArtAttack.Model
{
    public interface IDummyProductModel
    {
        Task AddDummyProductAsync(string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate);
        Task DeleteDummyProduct(int id);
        Task UpdateDummyProductAsync(int id, string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate);
        Task<string> GetSellerNameAsync(int? sellerId);
        Task<DummyProduct> GetDummyProductByIdAsync(int productId);
    }
}