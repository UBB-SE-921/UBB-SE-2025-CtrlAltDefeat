using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.ViewModel
{
    internal interface IFinalizePurchaseViewModel
    {
        event PropertyChangedEventHandler PropertyChanged;

        Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID);
    }
}
