using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ArtAttack.ViewModel
{
    public interface IBorrowProductViewModel : INotifyPropertyChanged
    {
        // Product details properties
        string ProductName { get; set; }
        string PriceText { get; set; }
        string SellerText { get; set; }
        string TypeText { get; set; }
        string DatesText { get; set; }

        // Visibility properties
        Visibility IsBorrowVisible { get; set; }
        Visibility IsJoinWaitListVisible { get; set; }
        Visibility ShowLeaveWaitlistButton { get; set; }
        Visibility ShowUserPlaceInWaitlist { get; set; }

        // Property for the product status ("borrowed" or others)
        string ProductStatus { get; set; }

        // Command for joining the waitlist
        ICommand JoinWaitlistCommand { get; }

        // Method to load product details
        Task LoadProductDetailsAsync();
    }
}
