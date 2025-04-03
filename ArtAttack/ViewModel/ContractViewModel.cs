using ArtAttack.Domain;
using ArtAttack.Model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace ArtAttack.ViewModel
{
    public class ContractViewModel : IContractViewModel
    {
        private readonly IContractModel _model;

        /// <summary>
        /// Constructor for the ContractViewModel
        /// </summary>
        /// <param name="connectionString" type="string">The connection string to the database</param>
        public ContractViewModel(string connectionString)
        {
            _model = new ContractModel(connectionString);
        }

        /// <summary>
        /// Get a contract by its ID
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The contract with the given ID></returns>
        public async Task<IContract> GetContractByIdAsync(long contractId)
        {
            return await _model.GetContractByIdAsync(contractId);
        }

        /// <summary>
        /// Get all contracts
        /// </summary>
        /// <returns The list of contracts></returns>
        public async Task<List<IContract>> GetAllContractsAsync()
        {
            return await _model.GetAllContractsAsync();
        }

        /// <summary>
        /// Get the history of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The list of contracts that are related to the given contract></returns>
        public async Task<List<IContract>> GetContractHistoryAsync(long contractId)
        {
            return await _model.GetContractHistoryAsync(contractId);
        }

        /// <summary>
        /// Add a contract to the database
        /// </summary>
        /// <param name="contract" type="Contract">The contract to add</param>
        /// <param name="pdfFile" type="byte[]">The PDF file of the contract</param>
        /// <returns The added contract></returns>  
        public async Task<IContract> AddContractAsync(IContract contract, byte[] pdfFile)
        {
            return await _model.AddContractAsync(contract, pdfFile);
        }

        /// <summary>
        /// Get the seller of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The ID and name of the seller></returns>
        public async Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId)
        {
            return await _model.GetContractSellerAsync(contractId);
        }

        /// <summary>
        /// Get the buyer of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The ID and name of the buyer></returns>
        public async Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId)
        {
            return await _model.GetContractBuyerAsync(contractId);
        }

        /// <summary>
        /// Get the order summary information of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The order summary information></returns>
        public async Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId)
        {
            return await _model.GetOrderSummaryInformationAsync(contractId);
        }

        /// <summary>
        /// Get the product details of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The product details></returns>
        public async Task<(DateTime StartDate, DateTime EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId)
        {
            return await _model.GetProductDetailsByContractIdAsync(contractId);
        }

        /// <summary>
        /// Get the contracts of a buyer
        /// </summary>
        /// <param name="buyerId" type="int">The ID of the buyer</param>
        /// <returns The list of contracts of the buyer></returns>
        public async Task<List<IContract>> GetContractsByBuyerAsync(int buyerId)
        {
            return await _model.GetContractsByBuyerAsync(buyerId);
        }

        /// <summary>
        /// Get the predefined contract by its type
        /// </summary>
        /// <param name="predefinedContractType" type="PredefinedContractType">The type of the predefined contract</param>
        /// <returns The predefined contract with the given type></returns>
        public async Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType)
        {
            return await _model.GetPredefinedContractByPredefineContractTypeAsync(predefinedContractType);
        }

        /// <summary>
        /// Get the order details of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The payment method and order date></returns>
        public async Task<(string PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId)
        {
            return await _model.GetOrderDetailsAsync(contractId);
        }

        /// <summary>
        /// Get the delivery date of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The delivery date></returns>
        public async Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId)
        {
            return await _model.GetDeliveryDateByContractIdAsync(contractId);
        }

        /// <summary>
        /// Get the PDF of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The PDF of the contract></returns>
        public async Task<byte[]> GetPdfByContractIdAsync(long contractId)
        {
            return await _model.GetPdfByContractIdAsync(contractId);
        }

        /// <summary>
        /// Generate and save a contract
        /// </summary>
        /// <param name="contract" type="Contract">The contract to generate and save</param>
        /// <param name="predefinedContract" type="PredefinedContractType">The type of the predefined contract</param>
        /// <param name="fieldReplacements" type="Dictionary<string, string>">The field replacements for the contract</param>
        /// <returns The task></returns>
        /// <exception cref="ArgumentNullException" throws on="contract == null">Thrown when the contract is null</exception>
        private byte[] _GenerateContractPdf(
                IContract contract,
                IPredefinedContract predefinedContract,
                Dictionary<string, string> fieldReplacements)
        {
            // Validate inputs.
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));
            if (predefinedContract == null)
                throw new ArgumentNullException(nameof(predefinedContract));

            // Ensure fieldReplacements is not null.
            fieldReplacements ??= new Dictionary<string, string>();

            // Replace format variables in the content.
            string content = predefinedContract.Content;
            foreach (var pair in fieldReplacements)
            {
                content = content.Replace("{" + pair.Key + "}", pair.Value);
            }

            // Replace specific placeholders.
            content = content.Replace("{ContractID}", contract.ContractID.ToString());
            content = content.Replace("{OrderID}", contract.OrderID.ToString());
            content = content.Replace("{ContractStatus}", contract.ContractStatus);
            content = content.Replace("{AdditionalTerms}", contract.AdditionalTerms);

            // Set the QuestPDF license.
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(textStyle => textStyle.FontSize(12).FontFamily("Segoe UI"));

                    // Header section with title.
                    page.Header().Element(header =>
                    {
                        // Apply container-wide styling and combine multiple elements inside a Column
                        header
                            .PaddingBottom(10)
                            .BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Column(column =>
                            {
                                // The Column itself is the single child of the header container.
                                column.Item()
                                      .Text("Contract Document")
                                      .SemiBold()
                                      .FontSize(20)
                                      .AlignCenter();
                            });
                    });

                    // Content section.
                    page.Content().Element(contentContainer =>
                    {
                        // Apply padding and wrap the text in a Column container.
                        contentContainer
                            .PaddingVertical(10)
                            .Column(column =>
                            {
                                column.Item()
                                      .Text(content);
                                //.TextAlignment(TextAlignment.Justify);
                            });
                    });


                    // Footer section with generation date and page numbers.
                    page.Footer().Element(footer =>
                    {
                        footer
                        .PaddingTop(10)
                        .BorderTop(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Column(column =>
                            column.Item().Row(row =>
                            {
                                // Left part: Generation date.
                                row.RelativeItem()
                                   .Text($"Generated on: {DateTime.Now.ToShortDateString()}")
                                   .FontSize(10)
                                   .FontColor(Colors.Grey.Medium);

                                // Right part: Page numbering.
                                row.ConstantItem(100)
                                   .AlignRight()
                                   .Text(text =>
                                   {
                                       text.DefaultTextStyle(x => x.FontColor(Colors.Grey.Medium)
                                                                    .FontSize(10));
                                       text.Span("Page ");
                                       text.CurrentPageNumber();
                                       text.Span(" of ");
                                       text.TotalPages();
                                   });

                            }));

                    });
                });
            });

            // Generate and return the PDF as a byte array.
            return document.GeneratePdf();
        }

        /// <summary>
        /// Get the field replacements for a contract
        /// </summary>
        /// <param name="contract" type="Contract">The contract to get the field replacements for</param>
        /// <returns The field replacements></returns>
        private async Task<Dictionary<string, string>> _GetFieldReplacements(IContract contract)
        {
            var fieldReplacements = new Dictionary<string, string>();

            // Retrieve the product dates asynchronously.
            var productDetails = await GetProductDetailsByContractIdAsync(contract.ContractID);
            var buyerDetails = await GetContractBuyerAsync(contract.ContractID);
            var sellerDetails = await GetContractSellerAsync(contract.ContractID);
            var orderDetails = await GetOrderDetailsAsync(contract.ContractID);
            var orderSummaryData = await GetOrderSummaryInformationAsync(contract.ContractID);
            var deliveryDate = await GetDeliveryDateByContractIdAsync(contract.ContractID);

            if (productDetails.HasValue)
            {
                DateTime StartDate = productDetails.Value.StartDate;
                DateTime EndDate = productDetails.Value.EndDate;
                var LoanPeriod = (EndDate - StartDate).TotalDays;

                fieldReplacements["StartDate"] = StartDate.ToShortDateString();
                fieldReplacements["EndDate"] = EndDate.ToShortDateString();
                fieldReplacements["LoanPeriod"] = LoanPeriod.ToString();
                fieldReplacements["ProductDescription"] = productDetails.Value.name;
                fieldReplacements["Price"] = productDetails.Value.price.ToString();
                fieldReplacements["BuyerName"] = buyerDetails.BuyerName;
                fieldReplacements["SellerName"] = sellerDetails.SellerName;
                fieldReplacements["PaymentMethod"] = orderDetails.PaymentMethod;
                fieldReplacements["AgreementDate"] = StartDate.ToShortDateString();
                fieldReplacements["LateFee"] = orderSummaryData["warrantyTax"].ToString();
                fieldReplacements["DueDate"] = EndDate.ToShortDateString();
            }
            else
            {
                fieldReplacements["StartDate"] = "N/A";
                fieldReplacements["EndDate"] = "N/A";
                fieldReplacements["LoanPeriod"] = "N/A";
                fieldReplacements["ProductDescription"] = "N/A";
                fieldReplacements["Price"] = "N/A";
                fieldReplacements["BuyerName"] = "N/A";
                fieldReplacements["SellerName"] = "N/A";
                fieldReplacements["PaymentMethod"] = "N/A";
                fieldReplacements["AgreementDate"] = "N/A";
                fieldReplacements["LateFee"] = "N/A";
                fieldReplacements["DeliveryDate"] = "N/A";
            }

            return fieldReplacements;
        }

        /// <summary>
        /// Generate and save a contract asynchronously
        /// </summary>
        /// <param name="contract" type="Contract">The contract to generate and save</param>
        /// <param name="contractType" type="PredefinedContractType">The type of the predefined contract</param>
        /// <returns The task></returns>
        public async Task GenerateAndSaveContractAsync(IContract contract, PredefinedContractType contractType)
        {

            var predefinedContract = await GetPredefinedContractByPredefineContractTypeAsync(contractType);


            var fieldReplacements = await _GetFieldReplacements(contract);

            // Generate the PDF (synchronously) using the generated replacements.
            var pdfBytes = _GenerateContractPdf(contract, predefinedContract, fieldReplacements);

            // Determine the Downloads folder path.
            string downloadsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string fileName = $"Contract_{contract.ContractID}.pdf";
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            // Save the PDF file asynchronously.
            await File.WriteAllBytesAsync(filePath, pdfBytes);

            // Open the saved PDF file using Windows.Storage and Windows.System APIs.
            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
            await Launcher.LaunchFileAsync(file);
        }

        /// <summary>
        /// Generate a PDF and add a contract
        /// </summary>
        /// <param name="contract" type="Contract">The contract to generate and add</param>
        /// <param name="contractType" type="PredefinedContractType">The type of the predefined contract</param>
        /// <returns The task></returns>
        /// <exception cref="Exception" throws on="File already exists">Thrown when the file already exists</exception>
        public async Task GeneratePDFAndAddContract(IContract contract, PredefinedContractType contractType)
        {
            if(await GetPdfByContractIdAsync(contract.ContractID) != null)
            {
                throw new Exception("File already exists");
            }

            var predefinedContract = await GetPredefinedContractByPredefineContractTypeAsync(contractType);


            var fieldReplacements = await _GetFieldReplacements(contract);

            var pdfBytes = _GenerateContractPdf(contract, predefinedContract, fieldReplacements);

            await AddContractAsync(contract, pdfBytes);
        }
    }
}
