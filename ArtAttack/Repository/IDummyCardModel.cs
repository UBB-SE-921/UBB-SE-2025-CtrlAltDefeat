using System.Threading.Tasks;

namespace ArtAttack.Model
{
    public interface IDummyCardModel
    {
        Task DeleteCardAsync(string cardNumber);
        Task<float> GetCardBalanceAsync(string cardNumber);
        Task UpdateCardBalanceAsync(string cardNumber, float balance);
    }
}