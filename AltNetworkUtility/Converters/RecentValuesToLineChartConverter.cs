using System;
using System.Globalization;
using System.Linq;

using AltNetworkUtility.Models;

using Microcharts;

using Xamarin.Forms;

namespace AltNetworkUtility.Converters
{
    public class RecentValuesToLineChartConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not RecentValues<ulong> recentValues)
                return Binding.DoNothing;

            if (recentValues.Values.Count < 10)
                return Binding.DoNothing;

            var chartEntries = recentValues.Values.Select(v => new ChartEntry(v.Value)
            {
                Color = new SkiaSharp.SKColor(0, 200, 0)
            });

            return new LineChart
            {
                BackgroundColor = new SkiaSharp.SKColor(0, 0, 0, 0),
                Entries = chartEntries.ToArray(),
                IsAnimated = false,
                LineSize = 1
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
