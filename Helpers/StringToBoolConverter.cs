using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace EscobarMatias_ExamenParcial_AppMovil_ll.Helpers
{
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string str && !string.IsNullOrWhiteSpace(str);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
