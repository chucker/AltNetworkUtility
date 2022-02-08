using System;
using System.Collections.Generic;

namespace AltNetworkUtility.Tabs.Traceroute
{
    public class TracerouteOutputParser
    {
        static string Regex = @"(?<Hop>\d+)\s+(?<IPAddress>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})";

        public static bool TryParse(string newOutput, out List<System.Net.IPAddress> results)
        {
            results = new();

            if (string.IsNullOrWhiteSpace(newOutput))
                return false;

            foreach (var item in newOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                var match = System.Text.RegularExpressions.Regex.Match(newOutput, Regex);

                if (!match.Success)
                    continue;

                if (!System.Net.IPAddress.TryParse(match.Groups["IPAddress"].Value, out var ipAddress))
                    continue;

                results.Add(ipAddress);
            }

            return true;
        }
    }
}