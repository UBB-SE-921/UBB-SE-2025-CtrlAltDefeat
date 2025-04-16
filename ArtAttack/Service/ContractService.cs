using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model; // Add this using directive

namespace ArtAttack.Service
{
    // Make the class public and implement the interface
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;

        // Add constructor injection for the repository
        public ContractService(IContractRepository contractRepository)
        {
            _contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(contractRepository));
        }

        // Implement the interface methods by calling the repository
        public Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId)
        {
            return _contractRepository.GetContractBuyerAsync(contractId);
        }

        public Task<IContract> GetContractByIdAsync(long contractId)
        {
            return _contractRepository.GetContractByIdAsync(contractId);
        }

        public Task<List<IContract>> GetContractHistoryAsync(long contractId)
        {
            return _contractRepository.GetContractHistoryAsync(contractId);
        }

        public Task<List<IContract>> GetContractsByBuyerAsync(int buyerId)
        {
            return _contractRepository.GetContractsByBuyerAsync(buyerId);
        }

        public Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId)
        {
            return _contractRepository.GetContractSellerAsync(contractId);
        }

        public Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId)
        {
            return _contractRepository.GetDeliveryDateByContractIdAsync(contractId);
        }

        public Task<(string? PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId)
        {
            return _contractRepository.GetOrderDetailsAsync(contractId);
        }

        public Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId)
        {
            return _contractRepository.GetOrderSummaryInformationAsync(contractId);
        }

        public Task<byte[]> GetPdfByContractIdAsync(long contractId)
        {
            return _contractRepository.GetPdfByContractIdAsync(contractId);
        }

        public Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType)
        {
            return _contractRepository.GetPredefinedContractByPredefineContractTypeAsync(predefinedContractType);
        }

        public Task<(DateTime? StartDate, DateTime? EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId)
        {
            return _contractRepository.GetProductDetailsByContractIdAsync(contractId);
        }

        // Implement the newly added methods
        public Task<List<IContract>> GetAllContractsAsync()
        {
            return _contractRepository.GetAllContractsAsync();
        }

        public Task<IContract> AddContractAsync(IContract contract, byte[] pdfFile)
        {
            // Add null check for pdfFile if necessary, though repository might handle it
            if (pdfFile == null)
            {
                throw new ArgumentNullException(nameof(pdfFile));
            }
            return _contractRepository.AddContractAsync(contract, pdfFile);
        }
    }
}
