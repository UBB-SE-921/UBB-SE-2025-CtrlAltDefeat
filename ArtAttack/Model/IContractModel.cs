using ArtAttack.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtAttack.Model
{
    public interface IContractModel
    {
        Task<IContract> AddContractAsync(IContract contract, byte[] pdfFile);
        Task<List<IContract>> GetAllContractsAsync();
        Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId);
        Task<IContract> GetContractByIdAsync(long contractId);
        Task<List<IContract>> GetContractHistoryAsync(long contractId);
        Task<List<IContract>> GetContractsByBuyerAsync(int buyerId);
        Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId);
        Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId);
        Task<(string PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId);
        Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId);
        Task<byte[]> GetPdfByContractIdAsync(long contractId);
        Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType);
        Task<(DateTime StartDate, DateTime EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId);
    }
}