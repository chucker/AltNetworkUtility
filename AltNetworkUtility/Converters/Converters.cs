using LambdaConverters;

using System.Collections;
using System.Linq;

using Xamarin.Forms;

namespace AltNetworkUtility.Converters
{
    public static class Converters
    {
        public static readonly IValueConverter EnumerableToMultilineString =
            ValueConverter.Create<IEnumerable?, string>(vca =>
            {
                if (vca.Value == null)
                    return "";

                return string.Join("\n", vca.Value.Cast<object>());
            });

        public static readonly IValueConverter TrueIfNotNull =
            ValueConverter.Create<object?, bool>(vca => vca.Value is not null);
    }
}
