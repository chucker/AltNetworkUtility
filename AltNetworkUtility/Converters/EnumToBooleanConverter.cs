using System;

using Xamarin.Forms;

namespace AltNetworkUtility.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is not string parameterString)
                return false;

            if (!Enum.IsDefined(value?.GetType(), value))
                return false;

            object parameterValue = Enum.Parse(value?.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is not string parameterString)
                return default;

            return Enum.Parse(targetType, parameterString);
        }
    }
}
