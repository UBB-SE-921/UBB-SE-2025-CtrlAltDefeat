using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.ViewModel;
using Microsoft.UI.Text;
using System.IO;

namespace ArtAttack
{
    public sealed partial class OrderHistoryUI : Window
    {
        private readonly int _userId;
        private IOrderViewModel _orderViewModel;
        private IContractViewModel _contractViewModel;
        private Dictionary<int, string> orderProductCategoryTypes = new Dictionary<int, string>();

        /// <summary>
        /// Initializes a new instance of the OrderHistoryUI window.
        /// </summary>
        /// <param name="connectionString">The database connection string. Must not be null or empty.</param>
        /// <param name="userId">The ID of the user whose order history to display. Must be a positive integer.</param>
        /// <exception cref="ArgumentNullException">Thrown when connectionString is null.</exception>
        /// <exception cref="ArgumentException">Thrown when userId is less than or equal to zero.</exception>
        public OrderHistoryUI(string connectionString, int userId)
        {
            InitializeComponent();
            _userId = userId;
            _orderViewModel = new OrderViewModel(connectionString);
            _contractViewModel = new ContractViewModel(connectionString);
            this.Activated += Window_Activated;
        }

        /// <summary>
        /// Event handler triggered when the window is activated. Loads initial order data.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">Event data.</param>
        private async void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            this.Activated -= Window_Activated;
            await LoadOrders(SearchTextBox.Text);
        }

        /// <summary>
        /// Loads orders for the current user based on search text and selected time period filter.
        /// </summary>
        /// <param name="searchText">Optional search text to filter orders by product name. Can be null.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when there is an error retrieving or displaying order data.</exception>
        private async Task LoadOrders(string searchText = null)
        {
            try
            {
                var selectedPeriod = (TimePeriodComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                // Use the view model to get orders with product info
                var orderDisplayInfos = await _orderViewModel.GetOrdersWithProductInfoAsync(_userId, searchText, selectedPeriod);

                // Extract the product category types for use in showing contract details
                foreach (var orderInfo in orderDisplayInfos)
                {
                    orderProductCategoryTypes[orderInfo.OrderSummaryID] = orderInfo.ProductCategory;
                }

                if (orderDisplayInfos.Count > 0)
                {
                    OrdersListView.ItemsSource = orderDisplayInfos;
                    OrdersListView.Visibility = Visibility.Visible;
                    NoResultsText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    OrdersListView.Visibility = Visibility.Collapsed;
                    NoResultsText.Visibility = Visibility.Visible;

                    NoResultsText.Text = string.IsNullOrEmpty(searchText) ?
                        "No orders found" :
                        $"No orders found containing '{searchText}'";

                    if (selectedPeriod != "All Orders")
                        NoResultsText.Text += $" in {selectedPeriod}";
                }
            }
            catch (Exception ex)
            {
                OrdersListView.Visibility = Visibility.Collapsed;
                NoResultsText.Visibility = Visibility.Visible;
                NoResultsText.Text = "Error loading orders";

                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to load orders: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Event handler for the refresh button click. Reloads the order list.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadOrders(SearchTextBox.Text);
        }

        /// <summary>
        /// Event handler for the order details button click. Shows detailed information for a selected order.
        /// </summary>
        /// <param name="sender">The source of the event (Button with Tag=orderSummaryId).</param>
        /// <param name="e">Event data.</param>
        /// <exception cref="Exception">Thrown when there is an error retrieving or displaying order details.</exception>
        private async void OrderDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderSummaryId)
            {
                try
                {
                    OrderDetailsDialog.XamlRoot = this.Content.XamlRoot;
                    button.Content = "Loading...";
                    button.IsEnabled = false;

                    var orderSummary = await _orderViewModel.GetOrderSummaryAsync(orderSummaryId);
                    OrderDetailsContent.Children.Clear();

                    AddDetailRow("Order Summary ID:", orderSummary.ID.ToString());
                    AddDetailRow("Subtotal:", orderSummary.Subtotal.ToString("C"));
                    AddDetailRow("Delivery Fee:", orderSummary.DeliveryFee.ToString("C"));
                    AddDetailRow("Final Total:", orderSummary.FinalTotal.ToString("C"));
                    AddDetailRow("Customer Name:", orderSummary.FullName);
                    AddDetailRow("Email:", orderSummary.Email);
                    AddDetailRow("Phone:", orderSummary.PhoneNumber);
                    AddDetailRow("Address:", orderSummary.Address);
                    AddDetailRow("Postal Code:", orderSummary.PostalCode);

                    if (!string.IsNullOrEmpty(orderSummary.AdditionalInfo))
                        AddDetailRow("Additional Info:", orderSummary.AdditionalInfo);

                    if (orderProductCategoryTypes.TryGetValue(orderSummary.ID, out string productType) && productType == "borrowed")
                    {
                        AddDetailRow("Warranty Tax:", orderSummary.WarrantyTax.ToString("C"));

                        if (!string.IsNullOrEmpty(orderSummary.ContractDetails))
                            AddDetailRow("Contract Details:", orderSummary.ContractDetails);

                        var viewContractButton = new Button
                        {
                            Content = "View Contract PDF",
                            Margin = new Thickness(0, 10, 0, 0),
                            HorizontalAlignment = HorizontalAlignment.Left
                        };

                        viewContractButton.Click += async (s, args) =>
                        {
                            try
                            {
                                var contract = await _contractViewModel.GetContractByIdAsync(orderSummary.ID);

                                var contractTypeValues = Enum.GetValues(typeof(PredefinedContractType));
                                PredefinedContractType firstContractType = default;
                                if (contractTypeValues.Length > 0)
                                {
                                    firstContractType = (PredefinedContractType)contractTypeValues.GetValue(0);
                                }

                                var predefinedContract = await _contractViewModel
                                    .GetPredefinedContractByPredefineContractTypeAsync(firstContractType);

                                var fieldReplacements = new Dictionary<string, string>
                                {
                                    {"CustomerName", orderSummary.FullName},
                                    {"ProductName", "Borrowed Product"},
                                    {"StartDate", DateTime.Now.ToString("yyyy-MM-dd")},
                                    {"EndDate", DateTime.Now.AddMonths(3).ToString("yyyy-MM-dd")},
                                    {"Price", orderSummary.FinalTotal.ToString("C")}
                                };
                            }
                            catch (Exception ex)
                            {
                                await ShowMessageAsync("Error", $"Failed to generate contract: {ex.Message}");
                            }
                        };

                        OrderDetailsContent.Children.Add(viewContractButton);
                    }

                    await OrderDetailsDialog.ShowAsync();
                }
                catch (Exception ex)
                {
                    await ShowMessageAsync("Error", ex.Message);
                }
                finally
                {
                    button.Content = "See Details";
                    button.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Shows a PDF document in a dialog.
        /// </summary>
        /// <param name="pdfBytes">The PDF document as a byte array. Must not be null or empty.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="IOException">Thrown when there is an error writing the PDF to a temporary file.</exception>
        /// <exception cref="ArgumentNullException">Thrown when pdfBytes is null.</exception>
        private async Task ShowPdfDialog(byte[] pdfBytes)
        {
            var contractFilePath = Path.Combine(Path.GetTempPath(), $"contract_{Guid.NewGuid()}.pdf");
            await File.WriteAllBytesAsync(contractFilePath, pdfBytes);

            var pdfDialog = new ContentDialog
            {
                Title = "Contract PDF",
                CloseButtonText = "Close",
                XamlRoot = this.Content.XamlRoot,
                Content = new WebView2
                {
                    Width = 800,
                    Height = 1000,
                    Source = new Uri(contractFilePath)
                }
            };

            await pdfDialog.ShowAsync();

            try { File.Delete(contractFilePath); } catch { }
        }

        /// <summary>
        /// Displays a message dialog with the specified title and message.
        /// </summary>
        /// <param name="title">The title of the dialog. Must not be null.</param>
        /// <param name="message">The message to display. Must not be null.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the XamlRoot is not available.</exception>
        private async Task ShowMessageAsync(string title, string message)
        {
            if (this.Content.XamlRoot == null) return;

            var messageDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot,
            };

            await messageDialog.ShowAsync();
        }

        /// <summary>
        /// Adds a row with label and value to the order details dialog.
        /// </summary>
        /// <param name="label">The label to display. Must not be null.</param>
        /// <param name="value">The value to display. Must not be null.</param>
        private void AddDetailRow(string label, string value)
        {
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
            stackPanel.Children.Add(new TextBlock { Text = label, FontWeight = FontWeights.SemiBold, Width = 150 });
            stackPanel.Children.Add(new TextBlock { Text = value });
            OrderDetailsContent.Children.Add(stackPanel);
        }

        /// <summary>
        /// Event handler for the search text box text changed event. Filters orders based on search text.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await Task.Delay(300); // 300ms delay
            await LoadOrders(SearchTextBox.Text);
        }
    }
}
