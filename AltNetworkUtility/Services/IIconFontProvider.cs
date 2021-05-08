using System;
using System.Globalization;

using Xamarin.Forms;

namespace AltNetworkUtility.Services
{
    public interface IIconFontProvider
    {
        ImageSource GetImage(string name);
    }

    //public static class IconFontConverters
    //{
    //    public static readonly IValueConverter ToImageSource =
    //        ValueConverter.Create<string, ImageSource>(s => Xamarin.Forms.DependencyService.Get<IIconFontProvider>().GetImage(s));
    //}

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
