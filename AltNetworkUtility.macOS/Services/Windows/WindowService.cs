using System.Collections.Concurrent;

using AltNetworkUtility.Services.Windows;

using AppKit;

using CoreFoundation;

using CoreGraphics;

using Serilog;

#nullable enable

namespace AltNetworkUtility.macOS.Services.Windows
{
    public enum Window
    {
        AboutBox,
        MainWindow,
        Preferences
    }

    public abstract class WindowService : IWindowService
    {
        protected static readonly ConcurrentDictionary<Window, NSWindow> _SingleInstanceWindows = new();

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

        protected void SetNSWindowFrameFromXamFormsPage(NSWindow window, Xamarin.Forms.Page page)
        {
            if (page.WidthRequest == 0 || page.HeightRequest == 0)
                Log.Warning("Width or height weren't set");

            window.SetFrame(new CGRect(200, 200, page.WidthRequest, page.HeightRequest), true, true);
        }

        public void ResizeToFit(Xamarin.Forms.Page page)
        {
            if (!_SingleInstanceWindows.TryGetValue(Window, out var nsWindow))
                Log.Error("Couldn't find window");

            if (page.WidthRequest == 0 || page.HeightRequest == 0)
                Log.Warning("Width or height weren't set");

            nsWindow.SetFrame(new CGRect(nsWindow.Frame.Left, nsWindow.Frame.Top, page.WidthRequest, page.HeightRequest), true, true);
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
}
