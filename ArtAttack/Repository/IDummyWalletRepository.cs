using System.Threading.Tasks;

namespace ArtAttack.Repository
{
    public interface IDummyWalletRepository
    {
        Task<float> GetWalletBalanceAsync(int walletID);
        Task UpdateWalletBalance(int walletID, float balance);
    }
}
