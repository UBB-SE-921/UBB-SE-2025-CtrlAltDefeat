using System;
using Microsoft.UI.Xaml.Controls;

namespace ArtAttack.Utils
{
    public class NavigationService
    {
        private Frame mainFrame;

        public NavigationService(Frame mainFrame)
        {
            this.mainFrame = mainFrame;
        }

        public void NavigateTo(string pageName, object parameter = null)
        {
            if (mainFrame != null)
            {
                Type pageType = Type.GetType($"ArtAttack.Views.{pageName}");

                if (pageType != null)
                {
                    mainFrame.Navigate(pageType, parameter);
                }
            }
        }
    }
}