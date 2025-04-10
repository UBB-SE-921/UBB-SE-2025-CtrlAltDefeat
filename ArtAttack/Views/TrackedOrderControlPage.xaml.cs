using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace ArtAttack.Views
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TrackedOrderControlPage : Page
    {
        internal ITrackedOrderViewModel ViewModel { get; set; }
        public int TrackedOrderID { get; set; }
        internal List<OrderCheckpoint> Checkpoints { get; set; }

        internal TrackedOrderControlPage(ITrackedOrderViewModel viewModel, int trackedOrderID)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
            TrackedOrderID = trackedOrderID;
            Checkpoints = new List<OrderCheckpoint>();
            LoadOrderData();
        }

        /// <summary>
        /// Loads the order data for the specified tracked order ID
        /// </summary>
        private async void LoadOrderData()
        {
            var order = await ViewModel.GetTrackedOrderByIDAsync(TrackedOrderID);
            if (order != null)
            {
                var checkpoints = await ViewModel.GetAllOrderCheckpointsAsync(TrackedOrderID);
                checkpoints.Reverse();

                DataContext = new
                {
                    CurrentStatus = order.CurrentStatus,
                    TrackedOrderID = order.TrackedOrderID,
                    DeliveryAddress = order.DeliveryAddress,
                    EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                    OrderID = order.OrderID,
                    Checkpoints = checkpoints
                };
            }
        }

        /// <summary>
        /// Shows an error dialog with the specified error message
        /// </summary>
        /// <param name="errorMessage">The message to be displayed in the error dialog</param>
        /// <returns></returns>
        private async Task ShowErrorDialog(string errorMessage)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = errorMessage,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// Shows a success dialog with the specified message
        /// </summary>
        /// <param name="message">The message to be shown in the success dialog</param>
        /// <returns></returns>
        private async Task ShowSuccessDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Success",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Handles the click event for the revert last checkpoint button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RevertLastCheckpointButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                TrackedOrder order = await ViewModel.GetTrackedOrderByIDAsync(TrackedOrderID);
                await ViewModel.RevertToPreviousCheckpoint(order);
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"{ex.Message}");
            }
            finally
            {
                LoadOrderData();
            }
        }

        /// <summary>
        /// Handles the click event for the change estimated delivery date button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeEstimatedDeliveryDateButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (deliveryCalendarDatePicker.Visibility == Visibility.Collapsed)
            {
                deliveryCalendarDatePicker.Visibility = Visibility.Visible;
                deliveryCalendarDatePicker.MinDate = DateTime.Now;
                confirmChangeEstimatedDeliveryDateButton.Visibility = Visibility.Visible;
            }
            else
            {
                deliveryCalendarDatePicker.Visibility = Visibility.Collapsed;
                confirmChangeEstimatedDeliveryDateButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles the click event for the confirm change estimated delivery date button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConfirmChangeEstimatedDeliveryDateButton_Clicked(object sender, RoutedEventArgs e)
        {
            var pickedDate = deliveryCalendarDatePicker.Date;
            if (pickedDate != null)
            {
                try
                {
                    DateTime pickedDateTime = pickedDate.Value.DateTime;
                    DateOnly newEstimatedDeliveryDate = DateOnly.FromDateTime(pickedDateTime);
                    var order = await ViewModel.GetTrackedOrderByIDAsync(TrackedOrderID);
                    await ViewModel.UpdateTrackedOrderAsync(TrackedOrderID, newEstimatedDeliveryDate, order.CurrentStatus);
                }
                catch (Exception ex)
                {
                    await ShowErrorDialog($"{ex.Message}");
                }
                finally
                {
                    LoadOrderData();
                    deliveryCalendarDatePicker.Visibility = Visibility.Collapsed;
                    confirmChangeEstimatedDeliveryDateButton.Visibility = Visibility.Collapsed;
                    deliveryCalendarDatePicker.Date = null;
                }
            }
        }

        /// <summary>
        /// Handles the click event for the add new checkpoint button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNewCheckpointButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (AddDetails.Visibility == Visibility.Collapsed)
            {
                AddDetails.Visibility = Visibility.Visible;
            }
            else
            {
                AddDetails.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles the click event for the confirm add new checkpoint button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConfirmAddNewCheckpointButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                string location = LocationTextBoxAdd.Text;
                string description = DescriptionTextBoxAdd.Text;
                string status = (StatusComboBoxAdd.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(status))
                {
                    await ShowErrorDialog("Please fill in all fields.");
                    return;
                }

                await ViewModel.AddOrderCheckpointAsync(new OrderCheckpoint
                {
                    Timestamp = DateTime.Now,
                    Location = location,
                    Description = description,
                    Status = Enum.Parse<OrderStatus>(status),
                    TrackedOrderID = this.TrackedOrderID
                });

                await ShowSuccessDialog("Checkpoint added successfully.");
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Failed to add checkpoint./n" + ex.ToString());
            }
            finally
            {
                LoadOrderData();
                AddDetails.Visibility = Visibility.Collapsed;
                LocationTextBoxAdd.Text = string.Empty;
                DescriptionTextBoxAdd.Text = string.Empty;
                StatusComboBoxAdd.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Handles the click event for the update current checkpoint button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UpdateCurrentCheckpointButton_Clicked(object sender, RoutedEventArgs e)
        {
            TrackedOrder order;
            try
            {
                order = await ViewModel.GetTrackedOrderByIDAsync(TrackedOrderID);
            }
            catch (Exception ex)
            {
                await ShowErrorDialog(ex.ToString());
                return;
            }
            var lastCheckpoint = (await ViewModel.GetLastCheckpoint(order));

            if (lastCheckpoint == null)
            {
                await ShowErrorDialog("No checkpoint found.");
                return;
            }

            TimestampDatePicker.Date = lastCheckpoint.Timestamp.Date;
            TimestampTimePicker.Time = lastCheckpoint.Timestamp.TimeOfDay;
            LocationTextBoxUpdate.Text = lastCheckpoint.Location;
            DescriptionTextBoxUpdate.Text = lastCheckpoint.Description;

            foreach (ComboBoxItem item in StatusComboBoxUpdate.Items)
            {
                if (item.Content.ToString() == lastCheckpoint.Status.ToString())
                {
                    StatusComboBoxUpdate.SelectedItem = item;
                    break;
                }
            }

            TimestampDatePicker.MaxDate = lastCheckpoint.Timestamp.Date;

            if (UpdateDetails.Visibility == Visibility.Collapsed)
            {
                UpdateDetails.Visibility = Visibility.Visible;
            }
            else
            {
                UpdateDetails.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles the click event for the manual timestamp radio button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManualTimestampRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (DateTimePickers != null)
            {
                DateTimePickers.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Handles the click event for the auto timestamp radio button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoTimestampRadio_Checked(object sender, RoutedEventArgs e)
        {
            DateTimePickers.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Handles the click event for the confirm update current checkpoint button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConfirmUpdateCurrentCheckpointButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescriptionTextBoxUpdate.Text) || StatusComboBoxUpdate.SelectedItem == null)
            {
                await ShowErrorDialog("Please fill in all the required fields.");
                return;
            }

            DateTime timestamp;
            if (ManualTimestampRadio.IsChecked == true)
            {
                var pickedDate = TimestampDatePicker.Date;
                if (pickedDate == null)
                {
                    await ShowErrorDialog("Please fill in all fields.");
                    return;
                }
                DateTime pickedDateTime = pickedDate.Value.DateTime;
                timestamp = pickedDateTime + TimestampTimePicker.Time;
            }
            else
            {
                timestamp = DateTime.Now;
            }

            string location = LocationTextBoxUpdate.Text;
            string description = DescriptionTextBoxUpdate.Text;
            string status = (StatusComboBoxUpdate.SelectedItem as ComboBoxItem)?.Content.ToString();

            try
            {
                TrackedOrder order = await ViewModel.GetTrackedOrderByIDAsync(TrackedOrderID);

                var lastCheckpoint = await ViewModel.GetLastCheckpoint(order);
                if (lastCheckpoint == null)
                {
                    await ShowErrorDialog("No checkpoint found to update.");
                    return;
                }

                await ViewModel.UpdateOrderCheckpointAsync(
                    lastCheckpoint.CheckpointID,
                    timestamp,
                    location,
                    description,
                    Enum.Parse<OrderStatus>(status));

                await ShowSuccessDialog("Checkpoint updated successfully.");
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Failed to update checkpoint.\n" + ex.ToString());
            }
            finally
            {
                LoadOrderData();

                UpdateDetails.Visibility = Visibility.Collapsed;
                TimestampDatePicker.Date = null;
                TimestampTimePicker.Time = DateTime.Now.TimeOfDay;
                LocationTextBoxUpdate.Text = string.Empty;
                DescriptionTextBoxUpdate.Text = string.Empty;
                StatusComboBoxUpdate.SelectedIndex = -1;
            }
        }
    }
}