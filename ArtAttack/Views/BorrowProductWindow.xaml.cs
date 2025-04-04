namespace ArtAttack
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using ArtAttack.Domain;
    using ArtAttack.Services;
    using ArtAttack.ViewModel;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public sealed partial class BorrowProductWindow : Window
    {
        //private BorrowProductViewModel ViewModel { get; set; }
        private readonly MessageDialogService _dialogService;
        private readonly int _currentUserID = 1;
        private readonly int _currentProductID = 2;

        public BorrowProductWindow(string connectionString, int productId)
        {
            this.InitializeComponent();

            // Create the ViewModel using the interface
            IBorrowProductViewModel viewModel = new BorrowProductViewModel(
                currentUserId: _currentUserID,
                currentProductId: _currentProductID,
                dialogService: _dialogService
            );

            // Set DataContext for the window
            MainGrid.DataContext = viewModel;

            viewModel.LoadProductDetailsAsync();
        }

    }
}