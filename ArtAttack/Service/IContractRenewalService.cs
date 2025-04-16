using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.Service
{
    public interface IContractRenewalService
    {
        Task AddRenewedContractAsync(IContract contract, byte[] pdfFile);
        Task<List<IContract>> GetRenewedContractsAsync();
        Task<bool> HasContractBeenRenewedAsync(long contractId);
    }
}
