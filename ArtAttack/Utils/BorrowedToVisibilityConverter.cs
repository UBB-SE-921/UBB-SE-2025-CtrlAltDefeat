using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ArtAttack.Utils
{
    partial class BorrowedToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value != null && value.ToString() == "borrowed" ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
