using System.Threading.Tasks;

namespace ArtAttack.Model
{
    public interface IDummyWalletModel
    {
        Task<float> GetWalletBalanceAsync(int walletID);
        Task UpdateWalletBalance(int walletID, float balance);
    }
}