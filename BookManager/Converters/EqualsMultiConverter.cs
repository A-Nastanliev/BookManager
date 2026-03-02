using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BookManager.Converters
{
    public class EqualsMultiConverter : IMultiValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return false;

            bool result = object.Equals(values[0], values[1]);
            return Invert ? !result : result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
