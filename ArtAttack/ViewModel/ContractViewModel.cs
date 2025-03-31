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
        private readonly ContractModel _model;

        public ContractViewModel(string connectionString)
        {
            _model = new ContractModel(connectionString);
        }

        public async Task<Contract> GetContractByIdAsync(long contractId)
        {
            return await _model.GetContractByIdAsync(contractId);
        }

        public async Task<List<Contract>> GetAllContractsAsync()
        {
            return await _model.GetAllContractsAsync();
        }

        public async Task<List<Contract>> GetContractHistoryAsync(long contractId)
        {
            return await _model.GetContractHistoryAsync(contractId);
        }

        public async Task<Contract> AddContractAsync(Contract contract, byte[] pdfFile)
        {
            return await _model.AddContractAsync(contract, pdfFile);
        }

        public async Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId)
        {
            return await _model.GetContractSellerAsync(contractId);
        }

        public async Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId)
        {
            return await _model.GetContractBuyerAsync(contractId);
        }

        public async Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId)
        {
            return await _model.GetOrderSummaryInformationAsync(contractId);
        }

        public async Task<(DateTime StartDate, DateTime EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId)
        {
            return await _model.GetProductDetailsByContractIdAsync(contractId);
        }

        public async Task<List<Contract>> GetContractsByBuyerAsync(int buyerId)
        {
            return await _model.GetContractsByBuyerAsync(buyerId);
        }

        public async Task<PredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType)
        {
            return await _model.GetPredefinedContractByPredefineContractTypeAsync(predefinedContractType);
        }

        public async Task<(string PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId)
        {
            return await _model.GetOrderDetailsAsync(contractId);
        }

        public async Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId)
        {
            return await _model.GetDeliveryDateByContractIdAsync(contractId);
        }

        public async Task<byte[]> GetPdfByContractIdAsync(long contractId)
        {
            return await _model.GetPdfByContractIdAsync(contractId);
        }

        private byte[] _GenerateContractPdf(
    Contract contract,
    PredefinedContract predefinedContract,
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
            content = content.Replace("{ContractID}", contract.ID.ToString());
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

        private async Task<Dictionary<string, string>> _GetFieldReplacements(Contract contract)
        {
            var fieldReplacements = new Dictionary<string, string>();

            // Retrieve the product dates asynchronously.
            var productDetails = await GetProductDetailsByContractIdAsync(contract.ID);
            var buyerDetails = await GetContractBuyerAsync(contract.ID);
            var sellerDetails = await GetContractSellerAsync(contract.ID);
            DateTime StartDate = productDetails.Value.StartDate;
            DateTime EndDate = productDetails.Value.EndDate;
            var LoanPeriod = (EndDate - StartDate).TotalDays;
            var orderDetails = await GetOrderDetailsAsync(contract.ID);
            var orderSummaryData = await GetOrderSummaryInformationAsync(contract.ID);
            var deliveryDate = await GetDeliveryDateByContractIdAsync(contract.ID);


            if (productDetails.HasValue)
            {
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


        public async Task GenerateAndSaveContractAsync(Contract contract, PredefinedContractType contractType)
        {

            var predefinedContract = await GetPredefinedContractByPredefineContractTypeAsync(contractType);


            var fieldReplacements = await _GetFieldReplacements(contract);

            // Generate the PDF (synchronously) using the generated replacements.
            var pdfBytes = _GenerateContractPdf(contract, predefinedContract, fieldReplacements);

            // Determine the Downloads folder path.
            string downloadsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string fileName = $"Contract_{contract.ID}.pdf";
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            // Save the PDF file asynchronously.
            await File.WriteAllBytesAsync(filePath, pdfBytes);

            // Open the saved PDF file using Windows.Storage and Windows.System APIs.
            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
            await Launcher.LaunchFileAsync(file);
        }

        public async Task GeneratePDFAndAddContract(Contract contract, PredefinedContractType contractType)
        {
            if(await GetPdfByContractIdAsync(contract.ID) != null)
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
