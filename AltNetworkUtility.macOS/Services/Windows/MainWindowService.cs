using AltNetworkUtility.MainWindow;

using AppKit;

using Xamarin.Forms.Platform.MacOS;

#nullable enable

namespace AltNetworkUtility.macOS.Services.Windows
{
    public class MainWindowService : WindowService
    {
        public override Window Window => Window.MainWindow;

        public override NSWindow OpenWindow()
        {
            return _SingleInstanceWindows.GetOrAdd(Window, _ =>
            {
                const NSWindowStyle windowStyle = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

                var window = new NSWindow
                {
                    BackingType = NSBackingStore.Buffered,
                    Level = NSWindowLevel.Normal,
                    StyleMask = windowStyle,
                    Title = AppDelegate.AppName
                };

                window.TabbingMode = NSWindowTabbingMode.Disallowed;

                var page = new MainPage();

                SetNSWindowFrameFromXamFormsPage(window, page);

                window.ContentView = page.CreateViewController().View;

                var windowController = new NSWindowController(window);
                var appDelegate = NSApplication.SharedApplication.Delegate as AppDelegate;
                windowController.ShowWindow(appDelegate);

                ShowWindow(window);

                return window;
            });
        }
    }
}
