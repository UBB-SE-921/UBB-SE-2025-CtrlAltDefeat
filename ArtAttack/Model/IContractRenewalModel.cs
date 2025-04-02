using ArtAttack.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtAttack.Model
{
    public interface IContractRenewalModel
    {
        Task AddRenewedContractAsync(IContract contract, byte[] pdfFile);
        Task<List<IContract>> GetRenewedContractsAsync();
        Task<bool> HasContractBeenRenewedAsync(long contractId);
    }
}