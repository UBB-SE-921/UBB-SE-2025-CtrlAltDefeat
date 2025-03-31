using ArtAttack.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtAttack.ViewModel
{
    interface IFinalizePurchaseViewModel
    {
        Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID);
    }
}
