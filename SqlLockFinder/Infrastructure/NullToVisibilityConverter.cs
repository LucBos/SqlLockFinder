using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SqlLockFinder.Infrastructure
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }

        public NullToVisibilityConverter()
        {
            // set defaults
            TrueValue = Visibility.Collapsed;
            FalseValue = Visibility.Visible;
        }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return value == null ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}