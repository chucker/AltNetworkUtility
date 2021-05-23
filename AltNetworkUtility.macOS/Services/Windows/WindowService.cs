using System.Collections.Concurrent;

using AppKit;

using CoreFoundation;

using CoreGraphics;

using Serilog;

using Xamarin.Forms.Platform.MacOS;

#nullable enable

namespace AltNetworkUtility.macOS.Services.Windows
{
    public enum Window
    {
        MainWindow,
        AboutBox
    }

    public abstract class WindowService
    {
        protected readonly ConcurrentDictionary<Window, NSWindow> _SingleInstanceWindows =
            new ConcurrentDictionary<Window, NSWindow>();

        readonly ILogger Log = Serilog.Log.ForContext<WindowService>();

        public abstract Window Window { get; }

        public abstract NSWindow OpenWindow();

        public void CloseWindow()
        {
            if (!_SingleInstanceWindows.TryGetValue(Window, out var nsWindow))
            {
                Log.Debug($"No window for {Window} was open in the first place.");

                return;
            }

            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                nsWindow.Close();
            });

            _SingleInstanceWindows.TryRemove(Window, out _);
        }

        public void ToggleWindow()
        {
            var window = _SingleInstanceWindows.GetOrAdd(Window, _ =>
            {
                var _window = OpenWindow();

                _window.IsVisible = false;

                return _window;
            });

            if (window.IsVisible)
                HideWindow(window);
            else
                ShowWindow(window);
        }

        protected static void ShowWindow(NSWindow window)
        {
            NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
            window.MakeKeyAndOrderFront(null);
        }

        private static void HideWindow(NSWindow window)
        {
            window.IsVisible = false;
        }
    }

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

                window.SetFrame(new CGRect(200, 200, 640, 480), true, true);
                window.Center();

                var page = new MainPage();

                window.ContentView = page.CreateViewController().View;

                NSWindowController windowController = new NSWindowController(window);
                var appDelegate = NSApplication.SharedApplication.Delegate as AppDelegate;
                windowController.ShowWindow(appDelegate);

                ShowWindow(window);

                return window;
            });
        }
    }
}
