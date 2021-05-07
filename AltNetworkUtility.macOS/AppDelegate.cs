using AppKit;

using Foundation;

using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace AltNetworkUtility.macOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        public override NSWindow MainWindow { get; }

        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;
            var rect = new CoreGraphics.CGRect(200, 200, 640, 388);
            MainWindow = new NSWindow(rect, style, NSBackingStore.Buffered, false)
            {
                Title = "Alt Network Utility", // choose your own Title here
                TitleVisibility = NSWindowTitleVisibility.Visible
            };
        }


        public override void DidFinishLaunching(NSNotification notification)
        {
            Forms.Init();

            LoadApplication(new App());

            base.DidFinishLaunching(notification);
        }

        public override void WillTerminate(NSNotification notification) { }
    }
}
