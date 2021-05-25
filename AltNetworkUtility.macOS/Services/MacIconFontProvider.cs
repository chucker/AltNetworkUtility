using AltNetworkUtility.macOS.Extensions;
using AltNetworkUtility.Services.IconFont;

using AppKit;

using CoreGraphics;

using Xamarin.Forms;

namespace AltNetworkUtility.macOS.Services
{
    public class MacIconFontProvider : IIconFontProvider
    {
        public ImageSource GetImage(string name, Color? color, Size? size)
            => ImageSource.FromStream(() =>
            {
                NSImage image = NSImage.GetSystemSymbol(name, null);

                if (color != null)
                    image = image.WithTintColor(color.Value);
                
                if (size != null)
                    image = image.WithChangedSize(new CGSize(size.Value.Width, size.Value.Height));

                return image.AsTiff().AsStream();
            });
    }
}
