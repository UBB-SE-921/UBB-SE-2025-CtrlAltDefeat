using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.ViewModel
{
    public interface IContractViewModel
    {
        Task<IContract> GetContractByIdAsync(long contractId);
        Task<List<IContract>> GetAllContractsAsync();
        Task<List<IContract>> GetContractHistoryAsync(long contractId);
        Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId);
        Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId);
        Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId);
        Task<(DateTime StartDate, DateTime EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId);
        Task<List<IContract>> GetContractsByBuyerAsync(int buyerId);
        Task<IContract> AddContractAsync(IContract contract, byte[] pdfFile);
        Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType);
        Task<(string PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId);
        Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId);
        Task<byte[]> GetPdfByContractIdAsync(long contractId);
        Task GenerateAndSaveContractAsync(IContract contract, PredefinedContractType contractType);
    }
}
