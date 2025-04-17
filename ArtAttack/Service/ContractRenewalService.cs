using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Repository; // Added using for Repository namespace

namespace ArtAttack.Service
{
    public class ContractRenewalService : IContractRenewalService
    {
        private readonly IContractRenewalRepository contractRenewalRepository; // Added repository field

        // Added constructor for dependency injection
        public ContractRenewalService(IContractRenewalRepository contractRenewalRepository)
        {
            contractRenewalRepository = contractRenewalRepository ?? throw new ArgumentNullException(nameof(contractRenewalRepository));
        }

        // Implemented interface methods by calling repository methods
        public Task AddRenewedContractAsync(IContract contract, byte[] pdfFile)
        {
            // You might add business logic here before or after calling the repository
            return contractRenewalRepository.AddRenewedContractAsync(contract, pdfFile);
        }

        public Task<List<IContract>> GetRenewedContractsAsync()
        {
            // You might add business logic here
            return contractRenewalRepository.GetRenewedContractsAsync();
        }

        public Task<bool> HasContractBeenRenewedAsync(long contractId)
        {
            // You might add business logic here
            return contractRenewalRepository.HasContractBeenRenewedAsync(contractId);
        }
    }
}
