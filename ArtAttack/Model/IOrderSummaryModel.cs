using ArtAttack.Domain;
using System.Threading.Tasks;

namespace ArtAttack.Model
{
    public interface IOrderSummaryModel
    {
        Task AddOrderSummaryAsync(float subtotal, float warrantyTax, float deliveryFee, float finalTotal, string fullName, string email, string phoneNumber, string address, string postalCode, string additionalInfo, string contractDetails);
        Task DeleteOrderSummaryAsync(int id);
        Task<OrderSummary> GetOrderSummaryByIDAsync(int orderSummaryID);
        Task UpdateOrderSummaryAsync(int id, float subtotal, float warrantyTax, float deliveryFee, float finalTotal, string fullName, string email, string phoneNumber, string address, string postalCode, string additionalInfo, string contractDetails);
    }
}