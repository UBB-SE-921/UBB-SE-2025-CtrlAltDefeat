using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Repository;
using ArtAttack.Service;
using ArtAttack.Shared;

namespace ArtAttack.Model
{
    [Obsolete("This class is deprecated. Please use DummyProductRepository and DummyProductService instead.")]
    public class DummyProductModel : IDummyProductModel
    {
        private readonly IDummyProductRepository dummyProductRepository;

        [ExcludeFromCodeCoverage]
        public DummyProductModel(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        public DummyProductModel(string connectionString, IDatabaseProvider databaseProvider)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (databaseProvider == null)
            {
                throw new ArgumentNullException(nameof(databaseProvider));
            }

            dummyProductRepository = new DummyProductRepository(connectionString, databaseProvider);
        }

        public async Task AddDummyProductAsync(string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            await dummyProductRepository.AddDummyProductAsync(name, price, sellerId, productType, startDate, endDate);
        }

        public async Task UpdateDummyProductAsync(int id, string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            await dummyProductRepository.UpdateDummyProductAsync(id, name, price, sellerId, productType, startDate, endDate);
        }

        public async Task DeleteDummyProduct(int id)
        {
            await dummyProductRepository.DeleteDummyProduct(id);
        }

        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            return await dummyProductRepository.GetSellerNameAsync(sellerId);
        }

        public async Task<DummyProduct> GetDummyProductByIdAsync(int productId)
        {
            return await dummyProductRepository.GetDummyProductByIdAsync(productId);
        }
    }
}