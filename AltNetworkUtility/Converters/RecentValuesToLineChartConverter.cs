using System;
using System.Globalization;
using System.Linq;

using AltNetworkUtility.Models;

using Microcharts;

using SkiaSharp;

using Xamarin.Forms;

namespace AltNetworkUtility.Converters
{
    public class RecentValuesToLineChartConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not NetworkInterfaceStatistics statistics)
                return Binding.DoNothing;

            if (statistics.RecvPackets.Values.Count < 3)
                return Binding.DoNothing;

            var series = new ChartSerie[]
            {
                //new ChartSerie
                //{
                //    Color = new SKColor(0x16, 0x69, 0x7A),
                //    Entries = statistics.SentPackets.Values.Select(v => new ChartEntry(v.Value)),
                //    Name = nameof(statistics.SentPackets)
                //},
                new ChartSerie
                {
                    Color = new SKColor(0xF0, 0x42, 0x76),
                    Entries = statistics.SentBytes.Deltas.Select(v => new ChartEntry((float)v * -1)),
                    Name = nameof(statistics.SentBytes)
                },
                //new ChartSerie
                //{
                //    Color = new SKColor(0x9C, 0x0D, 0x38),
                //    Entries = statistics.RecvPackets.Values.Select(v => new ChartEntry(v.Value)),
                //    Name = nameof(statistics.RecvPackets)
                //},
                new ChartSerie
                {
                    Color = new SKColor(0x26, 0xB3, 0xCF),
                    Entries = statistics.RecvBytes.Deltas.Select(v => new ChartEntry(v)),
                    Name = nameof(statistics.RecvBytes)
                },
            };

            return new LineChart
            {
                IsAnimated = false,
                LineMode = LineMode.Straight,
                LineSize = 1,
                PointMode = PointMode.None,
                Series = series,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
