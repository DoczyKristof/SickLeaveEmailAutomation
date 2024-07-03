using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SickLeaveEmailAutomation.WPF.Converters
{
    public class StringNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverse = parameter as string == "Inverse";
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return inverse ? Visibility.Collapsed : Visibility.Visible;
            }
            return inverse ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
