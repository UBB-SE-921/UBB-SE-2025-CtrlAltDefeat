using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.ViewModel
{
    public interface IBillingInfoModelView
    {
        Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID);
        void CalculateOrderTotal(int orderHistoryID);
        Task ApplyBorrowedTax(DummyProduct dummyProduct);
    }
}
