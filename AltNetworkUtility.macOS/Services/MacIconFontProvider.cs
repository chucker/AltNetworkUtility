using AltNetworkUtility.Services.IconFont;

using AppKit;

using CoreGraphics;

using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace AltNetworkUtility.macOS.Services
{
    public static class NSImageExtensions
    {
        public static NSImage WithTintColor(this NSImage image, Color color)
        {
            if (!image.Template)
                return image;

            var copiedImage = image.Copy() as NSImage;
            copiedImage.LockFocus();

            var tintColor = color.ToNSColor();
            tintColor.Set();

            var bounds = new CGRect(0, 0, copiedImage.Size.Width, copiedImage.Size.Height);
            NSGraphics.RectFill(bounds, NSCompositingOperation.SourceAtop);

            copiedImage.Template = false;
            copiedImage.UnlockFocus();

            return copiedImage;
        }
    }

    public class MacIconFontProvider : IIconFontProvider
    {
        public ImageSource GetImage(string name, Color? color)
            => ImageSource.FromStream(() =>
            {
                NSImage image = NSImage.GetSystemSymbol(name, null);

                if (color != null)
                    image = image.WithTintColor(color.Value);

                return image.AsTiff().AsStream();
            });
    }
}
