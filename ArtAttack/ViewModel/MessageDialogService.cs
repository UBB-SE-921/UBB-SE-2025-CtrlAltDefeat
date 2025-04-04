using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtAttack.ViewModel
{
    public class MessageDialogService : IMessageDialogService
    {
        private readonly FrameworkElement _xamlRootProvider;

        public MessageDialogService(FrameworkElement xamlRootProvider)
        {
            _xamlRootProvider = xamlRootProvider;
        }

        public async Task ShowMessageAsync(string title, string message)
        {
            if (_xamlRootProvider.XamlRoot == null)
            {
                throw new InvalidOperationException("XamlRoot is not initialized.");
            }

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = _xamlRootProvider.XamlRoot,
            };

            await dialog.ShowAsync();
        }
    }

}
