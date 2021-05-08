using System;
using System.Globalization;

using Xamarin.Forms;

namespace AltNetworkUtility.Services
{
    public interface IIconFontProvider
    {
        ImageSource GetImage(string name);
    }

    public class IconFontConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
                return DependencyService.Get<IIconFontProvider>().GetImage(s);

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
