using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.ViewModel
{
    internal interface IFinalizePurchaseViewModel
    {
        Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID);
    }
}
