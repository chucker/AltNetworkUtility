using Humanizer;

using LambdaConverters;

using System.Collections;

using Xamarin.Forms;

namespace AltNetworkUtility.Converters
{
    public static class Humanizer
    {
        public static readonly IValueConverter Pluralize =
            ValueConverter.Create<string, string, ICollection?>(vca =>
            {
                if (vca.Parameter?.Count != 1)
                    return vca.Value.Pluralize();

                return vca.Value;
            });
    }
}
