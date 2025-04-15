using System.Threading.Tasks;

namespace ArtAttack.Repository
{
    public interface IDummyCardRepository
    {
        Task DeleteCardAsync(string cardNumber);
        Task<float> GetCardBalanceAsync(string cardNumber);
        Task UpdateCardBalanceAsync(string cardNumber, float balance);
    }
}

