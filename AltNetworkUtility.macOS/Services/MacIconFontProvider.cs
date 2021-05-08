using AltNetworkUtility.Services;

using AppKit;

using Xamarin.Forms;

namespace AltNetworkUtility.macOS.Services
{
    public class MacIconFontProvider : IIconFontProvider
    {
        public ImageSource GetImage(string name)
            => ImageSource.FromStream(() => NSImage.GetSystemSymbol(name, null).AsTiff().AsStream());
    }
}
