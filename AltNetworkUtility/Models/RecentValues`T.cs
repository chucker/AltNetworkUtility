using System;
using System.Collections.Generic;
using System.Linq;

namespace AltNetworkUtility.Models
{
    public class RecentValues<TValue> where TValue : struct
    {
        public List<(DateTime Timestamp, TValue Value)> Values { get; } = new(100);

        public int RetainSeconds { get; set; } = 10;

        public void EnqueueValue(TValue value)
        {
            Values.RemoveAll(v => v.Timestamp < DateTime.Now.AddSeconds(-RetainSeconds));

            Values.Add((DateTime.Now, value));
        }

        public IEnumerable<ulong> Deltas
        {
            get
            {
                // maybe this will be nicer in C# 10 or whatever
                if (typeof(TValue) != typeof(ulong))
                    throw new NotImplementedException();

                if (Values.Count < 2)
                    return Enumerable.Empty<ulong>();

                var values = Values.Select(v => (ulong)(object)v.Value).ToList();

                //return values.Skip(1).Select((next, index) =>
                //next - values[index - 1]);

                return values.Zip(values.Skip(1), (current, next) => next - current);
            }
        }

        public double DeltaPerSecond
        {
            get
            {
                // maybe this will be nicer in C# 10 or whatever
                if (typeof(TValue) != typeof(ulong))
                    throw new NotImplementedException();

                if (Values.Count < 2)
                    return default;

                var last = Values.Last();
                var first = Values.First();

                var valueDiff = (ulong)(object)last.Value - (ulong)(object)first.Value;
                var timestampDiff = (last.Timestamp - first.Timestamp).TotalSeconds;

                return valueDiff / timestampDiff;
            }
        }

        public string DeltaPerSecond_SI => _ConvertToSI(DeltaPerSecond);

        public TValue? MostRecentValue
            => Values.LastOrDefault().Value;

        public string MostRecentValue_SI => _ConvertToSI(MostRecentValue);

        public TValue? MovingAverage
        {
            get
            {
                if (!Values.Any())
                    return null;

                if (Values is List<(DateTime, double Value)> doubleQueue)
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
