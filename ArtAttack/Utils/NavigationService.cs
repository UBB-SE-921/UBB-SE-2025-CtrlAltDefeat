using Microsoft.UI.Xaml.Controls;
using System;

namespace ArtAttack.Utils
{
    public class NavigationService
    {
        private Frame _mainFrame;

        public NavigationService(Frame mainFrame)
        {
            _mainFrame = mainFrame;
        }

        public void NavigateTo(string pageName, object parameter = null)
        {
            if (_mainFrame != null)
            {
                Type pageType = Type.GetType($"ArtAttack.Views.{pageName}");

                if (pageType != null)
                {
                    _mainFrame.Navigate(pageType, parameter);
                }
            }
        }
    }
}