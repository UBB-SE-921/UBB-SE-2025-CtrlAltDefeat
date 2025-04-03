using ArtAttack.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtAttack.Model
{
    public interface IOrderHistoryModel
    {
        Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID);
    }
}