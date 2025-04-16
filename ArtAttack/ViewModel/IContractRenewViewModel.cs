﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.ViewModel
{
    public interface IContractRenewViewModel
    {
        List<IContract> BuyerContracts { get; }
        IContract SelectedContract { get; }

        bool CanSellerApproveRenewal(int renewalCount);
        byte[] GenerateContractPdf(IContract contract, string content);
        Task<(DateTime? StartDate, DateTime? EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId);
        Task<bool> HasContractBeenRenewedAsync();
        Task<int> InsertPdfAsync(byte[] fileBytes);
        bool IsProductAvailable(int productId);
        Task<bool> IsRenewalPeriodValidAsync();
        Task LoadContractsForBuyerAsync(int buyerID);
        Task SelectContractAsync(long contractID);
        Task<(bool Success, string Message)> SubmitRenewalRequestAsync(DateTime newEndDate, int buyerID, int productID, int sellerID);
    }
}