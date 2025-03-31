using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.Data.SqlClient;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArtAttack.ViewModel
{
    public class ContractRenewViewModel
    {
        private readonly ContractModel _contractModel;
        private readonly ContractRenewalModel _renewalModel;
        private readonly NotificationDataAdapter _notificationAdapter;
        private readonly string _connectionString;

        public List<Contract> BuyerContracts { get; private set; }
        public Contract SelectedContract { get; private set; } = null!;

        public ContractRenewViewModel(string connectionString)
        {
            _contractModel = new ContractModel(connectionString);
            _renewalModel = new ContractRenewalModel(connectionString);
            _notificationAdapter = new NotificationDataAdapter(connectionString);
            _connectionString = connectionString;
            BuyerContracts = new List<Contract>();
        }

        /// <summary>
        /// Loads all contracts for the given buyer and filters them to include only those with status "ACTIVE" or "RENEWED".
        /// </summary>
        public async Task LoadContractsForBuyerAsync(int buyerID)
        {
            // Load all contracts for the buyer
            var allContracts = await _contractModel.GetContractsByBuyerAsync(buyerID);

            // Filter the contracts to include only those with status "ACTIVE" or "RENEWED"
            BuyerContracts = allContracts.Where(c => c.ContractStatus == "ACTIVE" || c.ContractStatus == "RENEWED").ToList();
        }

        /// <summary>
        /// Retrieves and sets the selected contract by its ID.
        /// </summary>
        public async Task SelectContractAsync(long contractID)
        {
            SelectedContract = await _contractModel.GetContractByIdAsync(contractID);
        }

        /// <summary>
        /// Retrieves the start and end dates of the product associated with a given contract.
        /// </summary>
        public async Task<(DateTime StartDate, DateTime EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId)
        {
            return await _contractModel.GetProductDetailsByContractIdAsync(contractId);
        }

        /// <summary>
        /// Checks whether the current date is within the valid renewal period (between 2 and 7 days before contract end).
        /// </summary>
        public async Task<bool> IsRenewalPeriodValidAsync()
        {
            var dates = await GetProductDetailsByContractIdAsync(SelectedContract.ID);
            if (dates == null) return false;

            DateTime oldEndDate = dates.Value.EndDate;
            DateTime currentDate = DateTime.Now.Date;
            int daysUntilEnd = (oldEndDate - currentDate).Days;

            return daysUntilEnd <= 7 && daysUntilEnd >= 2;
        }


        /// <summary>
        /// Simulates a check to determine if a product is available.
        /// </summary>
        public bool IsProductAvailable(int productId)
        {
            return true;
        }


        /// <summary>
        /// Simulates a check to determine if the seller can approve a renewal based on the renewal count.
        /// </summary>
        public bool CanSellerApproveRenewal(int renewalCount)
        {
            return renewalCount < 1;
        }

        /// <summary>
        /// Inserts a PDF file into the database and returns the newly generated PDF ID.
        /// </summary>
        private async Task<int> InsertPdfAsync(byte[] fileBytes)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("INSERT INTO PDF ([file]) OUTPUT INSERTED.ID VALUES (@file)", conn))
            {
                cmd.Parameters.AddWithValue("@file", fileBytes);
                await conn.OpenAsync();
                return (int)await cmd.ExecuteScalarAsync();
            }
        }

        /// <summary>
        /// Checks whether the currently selected contract has already been renewed.
        /// </summary>
        public async Task<bool> HasContractBeenRenewedAsync()
        {
            return await _renewalModel.HasContractBeenRenewedAsync(SelectedContract.ID);
        }

        /// <summary>
        /// Generates a PDF document containing the contract content.
        /// </summary>
        private byte[] GenerateContractPdf(Contract contract, string content)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));
                    page.Content().Text(content);
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Submits a request to renew the selected contract if all business rules are satisfied.
        /// Also generates and saves a new PDF and sends notifications.
        /// </summary>
        public async Task<(bool Success, string Message)> SubmitRenewalRequestAsync(DateTime newEndDate, int buyerID, int productID, int sellerID)
        {
            try
            {
                // Ensure a contract is selected before proceeding
                if (SelectedContract == null)
                    return (false, "No contract selected.");

                // Check if the contract was already renewed
                if (await HasContractBeenRenewedAsync())
                    return (false, "This contract has already been renewed.");

                // Validate the current date is within the renewal window (2 to 7 days before end)
                if (!await IsRenewalPeriodValidAsync())
                    return (false, "Contract is not in a valid renewal period (between 2 and 7 days before end date).");

                // Get the current contract's product dates
                var oldDates = await GetProductDetailsByContractIdAsync(SelectedContract.ID);
                if (!oldDates.HasValue)
                    return (false, "Could not retrieve current contract dates.");

                // Ensure the new end date is after the old one
                if (newEndDate <= oldDates.Value.EndDate)
                    return (false, "New end date must be after the current end date.");

                // Check if product is available for renewal
                if (!IsProductAvailable(productID))
                    return (false, "Product is not available.");

                // Check if seller allows renewal (based on renewal count)
                if (!CanSellerApproveRenewal(SelectedContract.RenewalCount))
                    return (false, "Renewal not allowed: seller limit exceeded.");

                // Build the updated contract content text
                string contractContent = $"Renewed Contract for Order {SelectedContract.OrderID}.\nOriginal Contract ID: {SelectedContract.ID}.\nNew End Date: {newEndDate:dd/MM/yyyy}";

                // Generate the contract PDF
                byte[] pdfBytes = GenerateContractPdf(SelectedContract, contractContent);

                // Insert the new PDF into the database and get its ID
                int newPdfId = await InsertPdfAsync(pdfBytes);

                // Save PDF locally in Downloads folder
                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string fileName = $"RenewedContract_{SelectedContract.ID}_to_{newEndDate:yyyyMMdd}.pdf";
                string filePath = Path.Combine(downloadsPath, fileName);
                await File.WriteAllBytesAsync(filePath, pdfBytes);

                // Prepare and insert the new renewed contract into the database
                var updatedContract = new Contract
                {
                    OrderID = SelectedContract.OrderID,
                    ContractStatus = "RENEWED",
                    ContractContent = contractContent,
                    RenewalCount = SelectedContract.RenewalCount + 1,
                    PredefinedContractID = SelectedContract.PredefinedContractID,
                    PDFID = newPdfId,
                    RenewedFromContractID = SelectedContract.ID
                };

                await _renewalModel.AddRenewedContractAsync(updatedContract, pdfBytes);

                // Send notifications to seller, buyer, and waitlist
                var now = DateTime.Now;
                _notificationAdapter.AddNotification(new ContractRenewalRequestNotification(sellerID, now, (int)SelectedContract.ID));
                _notificationAdapter.AddNotification(new ContractRenewalAnswerNotification(buyerID, now, (int)SelectedContract.ID, true));
                _notificationAdapter.AddNotification(new ContractRenewalWaitlistNotification(999, now, productID));

                return (true, "Contract renewed successfully!");
            }
            catch (Exception ex)
            {
                // Catch any unexpected errors and return an appropriate message
                return (false, $"Unexpected error: {ex.Message}");
            }
        }

    }
}
