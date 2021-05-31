using LambdaConverters;

using Xamarin.Forms;

namespace AltNetworkUtility.Services.IconFont
{
    public class IconSpec
    {
        public Color? Color { get; set; }
        public string Name { get; private set; }
        public Size? Size { get; set; }

        public IconSpec(string name) => Name = name;
    }

#nullable disable
    public class XamlIcon
    {
        public Color Color { get; set; }
        public string Name { get; set; }
        public Size Size { get; set; }

        public static implicit operator ImageSource(XamlIcon source)
        {
            var svc = DependencyService.Get<IIconFontProvider>();
            return svc.GetImage(source.Name, source.Color, source.Size);
        }
    }
#nullable restore

    public interface IIconFontProvider
    {
        ImageSource? GetImage(string name, Color? color, Size? size);
    }

    public static class Converters
    {
        public static readonly IValueConverter IconSpecToImageSource =
            ValueConverter.Create<IconSpec, ImageSource?>(vca =>
            {
                var spec = vca.Value;
                var svc = DependencyService.Get<IIconFontProvider>();
                return svc.GetImage(spec.Name, spec.Color, spec.Size);
            });
    }
}
