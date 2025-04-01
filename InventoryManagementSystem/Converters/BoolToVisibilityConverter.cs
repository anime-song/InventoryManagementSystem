using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace InventoryManagementSystem.WPF.Converters
{
    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; } = false;
        public bool Collapse { get; set; } = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool flag)
            {
                if (Invert)
                    flag = !flag;

                if (flag)
                    return Visibility.Visible;
                else
                    return Collapse ? Visibility.Collapsed : Visibility.Hidden;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }

            return DependencyProperty.UnsetValue;
        }
    }
}