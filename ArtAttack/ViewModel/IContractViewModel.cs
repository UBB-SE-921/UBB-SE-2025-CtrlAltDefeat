using ArtAttack.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtAttack.ViewModel
{
    public interface IContractViewModel
    {
        //ADD HERE
        Task<Contract> GetContractByIdAsync(long contractId);
        Task<List<Contract>> GetAllContractsAsync();
        Task<List<Contract>> GetContractHistoryAsync(long contractId);
        Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId);
        Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId);
        Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId);
        Task<(DateTime StartDate, DateTime EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId);
        Task<List<Contract>> GetContractsByBuyerAsync(int buyerId);
        Task<Contract> AddContractAsync(Contract contract, byte[] pdfFile);
        Task<PredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType);
        Task<(string PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId);
        Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId);
        Task<byte[]> GetPdfByContractIdAsync(long contractId);
        Task GenerateAndSaveContractAsync(Contract contract, PredefinedContractType contractType);
    }
}
