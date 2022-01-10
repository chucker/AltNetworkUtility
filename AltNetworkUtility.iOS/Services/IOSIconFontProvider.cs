using AltNetworkUtility.Services.IconFont;

using UIKit;

using Xamarin.Forms;

namespace AltNetworkUtility.iOS.Services
{
    public class IOSIconFontProvider : IIconFontProvider
    {
        public ImageSource GetImage(string name) => ImageSource.FromStream(() => UIImage.GetSystemImage(name).AsPNG().AsStream());

        public ImageSource GetImage(string name, Color? color, Size? size)
        {
            throw new System.NotImplementedException();
        }
    }
}
