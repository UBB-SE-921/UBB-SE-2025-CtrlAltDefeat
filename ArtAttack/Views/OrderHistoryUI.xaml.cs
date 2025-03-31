
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.ViewModel;
using Microsoft.UI.Text;
using ArtAttack.Model;
using System.IO;  

namespace ArtAttack
{
    public sealed partial class OrderHistoryUI : Window
    {
        private readonly string _connectionString;
        private readonly int _userId;
        private OrderViewModel _orderViewModel;
        private IContractViewModel _contractViewModel;

        Dictionary<int, string> types = new Dictionary<int, string>();


        public OrderHistoryUI(string connectionString, int userId)
        {
            InitializeComponent();
            _connectionString = connectionString;
            _userId = userId;
            _orderViewModel = new OrderViewModel(connectionString);
            this.Activated += Window_Activated;
        }

        private async void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            this.Activated -= Window_Activated;
            await LoadOrders(SearchTextBox.Text);
        }

        private async Task LoadOrders(string searchText = null)
        {
            try
            {
                List<Order> orders = new List<Order>();
                Dictionary<int, string> productNames = new Dictionary<int, string>();
                Dictionary<int, string> productTypeNames = new Dictionary<int, string>(); 

                var selectedPeriod = (TimePeriodComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"SELECT 
                o.OrderID, 
                p.name AS ProductName, 
                o.ProductType,  
                p.productType AS ProductTypeName,  
                o.OrderDate, 
                o.PaymentMethod, 
                o.OrderSummaryID
            FROM [Order] o
            JOIN [DummyProduct] p ON o.ProductType = p.ID
            WHERE o.BuyerID = @UserId";

                    if (!string.IsNullOrEmpty(searchText))
                        query += " AND p.name LIKE @SearchText";

                    if (selectedPeriod == "Last 3 Months")
                        query += " AND o.OrderDate >= DATEADD(month, -3, GETDATE())";
                    else if (selectedPeriod == "Last 6 Months")
                        query += " AND o.OrderDate >= DATEADD(month, -6, GETDATE())";
                    else if (selectedPeriod == "This Year")
                        query += " AND YEAR(o.OrderDate) = YEAR(GETDATE())";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", _userId);
                        if (!string.IsNullOrEmpty(searchText))
                            command.Parameters.AddWithValue("@SearchText", $"%{searchText}%");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var order = new Order
                                {
                                    OrderID = reader.GetInt32(0),
                                    ProductType = reader.GetInt32(2),
                                    OrderDate = reader.GetDateTime(4),
                                    PaymentMethod = reader.GetString(5),
                                    OrderSummaryID = reader.GetInt32(6)
                                };

                                orders.Add(order);
                                productNames[order.OrderID] = reader.GetString(1); 
                                productTypeNames[order.OrderID] = reader.GetString(3);

                                if (productTypeNames[order.OrderID] == "new" || productTypeNames[order.OrderID] == "used")
                                {
                                    types[order.OrderSummaryID] = "new";
                                }

                                else
                                {
                                    types[order.OrderSummaryID] = "borrowed";
                                }
                            }
                        }
                    }
                }

                
                if (orders.Count > 0)
                {
                   
                    var displayOrders = new List<object>();
                    foreach (var order in orders)
                    {
                        string productName;
                        if (!productNames.TryGetValue(order.OrderID, out productName))
                        {
                            productName = "Unknown";
                        }

                        string productTypeName;
                        if (!productTypeNames.TryGetValue(order.OrderID, out productTypeName))
                        {
                            productTypeName = "Unknown";
                        }

                        displayOrders.Add(new
                        {
                            OrderID = order.OrderID,
                            ProductName = productName,
                            ProductTypeName = productTypeName,
                            OrderDate = order.OrderDate.ToString("yyyy-MM-dd"),
                            PaymentMethod = order.PaymentMethod,
                            OrderSummaryID = order.OrderSummaryID
                        });
                    }

                    OrdersListView.ItemsSource = displayOrders;

                    OrdersListView.Visibility = Visibility.Visible;
                    NoResultsText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    OrdersListView.Visibility = Visibility.Collapsed;
                    NoResultsText.Visibility = Visibility.Visible;

                    
                    NoResultsText.Text = string.IsNullOrEmpty(searchText) ? "No orders found" : $"No orders found containing '{searchText}'";
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


        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadOrders(SearchTextBox.Text);
        }

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

                   
                    if (types.TryGetValue(orderSummary.ID, out string productType) && productType == "borrowed")
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
                               
                                var enumValues = Enum.GetValues(typeof(PredefinedContractType));
                                PredefinedContractType firstContractType = default; 
                                if (enumValues.Length > 0)
                                {
                                    firstContractType = (PredefinedContractType)enumValues.GetValue(0);
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

                                // Generate PDF
                                //byte[] pdfBytes = _contractViewModel.GenerateContractPdf(contract, predefinedContract, fieldReplacements);

                                // Show PDF in dialog
                                //await ShowPdfDialog(pdfBytes);
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

        private async Task ShowPdfDialog(byte[] pdfBytes)
        {
            
            var tempFile = Path.Combine(Path.GetTempPath(), $"contract_{Guid.NewGuid()}.pdf");
            await File.WriteAllBytesAsync(tempFile, pdfBytes);

           
            var pdfDialog = new ContentDialog
            {
                Title = "Contract PDF",
                CloseButtonText = "Close",
                XamlRoot = this.Content.XamlRoot,
                Content = new WebView2
                {
                    Width = 800,
                    Height = 1000,
                    Source = new Uri(tempFile)
                }
            };

            await pdfDialog.ShowAsync();

            try { File.Delete(tempFile); } catch { }
        }

        private async Task ShowMessageAsync(string title, string message)
        {
          
            if (this.Content.XamlRoot == null) return;

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot,

            };

            await dialog.ShowAsync();
        }
        private void AddDetailRow(string label, string value)
        {
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
            stackPanel.Children.Add(new TextBlock { Text = label, FontWeight = FontWeights.SemiBold, Width = 150 });
            stackPanel.Children.Add(new TextBlock { Text = value });
            OrderDetailsContent.Children.Add(stackPanel);
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
     
            await Task.Delay(300); // 300ms delay
            await LoadOrders(SearchTextBox.Text);
        }
    }
}