using AltNetworkUtility.About;
using AltNetworkUtility.Services.Windows;

using AppKit;

using Foundation;

using Xamarin.Forms.Platform.MacOS;

#nullable enable

namespace AltNetworkUtility.macOS.Services.Windows
{
    public class AboutBoxWindowService : WindowService, IAboutBoxWindowService
    {
        public override Window Window => Window.AboutBox;

        public override NSWindow OpenWindow()
        {
            return _SingleInstanceWindows.GetOrAdd(Window, _ =>
            {
                const NSWindowStyle windowStyle = NSWindowStyle.Closable | NSWindowStyle.Titled;

                var window = new NSWindow
                {
                    BackingType = NSBackingStore.Buffered,
                    Level = NSWindowLevel.Normal,
                    StyleMask = windowStyle,
                    Title = $"About {AppDelegate.AppName}"
                };

                window.TabbingMode = NSWindowTabbingMode.Disallowed;

                var page = new AboutBoxPage();

                SetNSWindowFrameFromXamFormsPage(window, page);
                
                window.ContentView = page.CreateViewController().View;

                var windowController = new NSWindowController(window);
                windowController.ShowWindow(null);

                window.Delegate = new WindowDelegate();

                ShowWindow(window);

                return window;
            });
        }

        private class WindowDelegate : NSWindowDelegate
        {
            public override void WillClose(NSNotification notification)
            {
                Xamarin.Forms.DependencyService.Get<AboutBoxWindowService>().CloseWindow();
            }
        }
    }
}
