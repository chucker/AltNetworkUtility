using System;
using System.Collections.Generic;
using System.Linq;

namespace AltNetworkUtility.Models
{
    public class RecentValues<TValue> where TValue : struct
    {
        private readonly List<(DateTime Timestamp, TValue Value)> _RecentValues = new(100);

        public int SmoothToSeconds { get; set; } = 10;

        public void EnqueueValue(TValue value)
        {
            _RecentValues.RemoveAll(v => v.Timestamp < DateTime.Now.AddSeconds(-SmoothToSeconds));

            _RecentValues.Add((DateTime.Now, value));
        }

        public double DeltaPerSecond
        {
            get
            {
                // maybe this will be nicer in C# 10 or whatever
                if (typeof(TValue) != typeof(ulong))
                    throw new NotImplementedException();

                if (_RecentValues.Count() < 2)
                    return default;

                var last = _RecentValues.Last();
                var first = _RecentValues.First();

                var valueDiff = (ulong)(object)last.Value - (ulong)(object)first.Value;
                var timestampDiff = (last.Timestamp - first.Timestamp).TotalSeconds;

                return valueDiff / timestampDiff;
            }
        }

        public string DeltaPerSecond_SI => _ConvertToSI(DeltaPerSecond);

        public TValue? MostRecentValue
            => _RecentValues.LastOrDefault().Value;

        public string MostRecentValue_SI => _ConvertToSI(MostRecentValue);

        public TValue? MovingAverage
        {
            get
            {
                if (!_RecentValues.Any())
                    return null;

                if (_RecentValues is List<(DateTime, double Value)> doubleQueue)
                    return (TValue?)(object?)doubleQueue.Select(v => v.Value).Average();

                throw new NotImplementedException();
            }
        }

        private static string _ConvertToSI(object? value)
        {
            if (value == null)
                return "";

            switch (value)
            {
                case ulong u:
                    return u switch
                    {
                        >= 1_000_000_000_000 => $"{u / 1_000_000_000_000:N2}T",
                        >= 1_000_000_000 => $"{u / 1_000_000_000:N2}G",
                        >= 1_000_000 => $"{u / 1_000_000:N2}M",
                        >= 1_000 => $"{u / 1_000:N2}k",
                        _ => $"{u:N0}"
                    };

                case double d:
                    return d switch
                    {
                        >= 1_000_000_000_000 => $"{d / 1_000_000_000_000:N2}T",
                        >= 1_000_000_000 => $"{d / 1_000_000_000:N2}G",
                        >= 1_000_000 => $"{d / 1_000_000:N2}M",
                        >= 1_000 => $"{d / 1_000:N2}k",
                        _ => $"{d:N0}"
                    };

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
