using System;
using System.Globalization;
using System.Windows.Data;

namespace SRKT.WPF.Converters
{
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool dostepny)
            {
                return dostepny ? "Dostępny" : "Zajęty";
            }
            return "Nieznany";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}