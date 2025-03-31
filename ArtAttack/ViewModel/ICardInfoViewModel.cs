using ArtAttack.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtAttack.ViewModel
{
    interface ICardInfoViewModel
    {
        Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID);

        Task ProcessCardPaymentAsync();


    }
}
