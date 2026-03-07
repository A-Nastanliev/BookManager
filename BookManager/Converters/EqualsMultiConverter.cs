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
            if (values == null || values.Length < 2)
                return false;

            var target = values[0];

            for (int i = 1; i < values.Length; i++)
            {
                if (object.Equals(target, values[i]))
                    return Invert ? false : true;
            }

            return Invert ? true : false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
