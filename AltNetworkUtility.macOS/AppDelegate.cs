using System;
using System.Linq;
using System.Reflection;

using AltNetworkUtility.macOS.Services;
using AltNetworkUtility.macOS.Services.Windows;
using AltNetworkUtility.Services;
using AltNetworkUtility.Services.IconFont;

using AppKit;

using Foundation;

using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

#nullable enable

namespace AltNetworkUtility.macOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        private NSWindow? _MainWindow;
        public override NSWindow? MainWindow => _MainWindow;

        public static string AppName => System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        public AppDelegate()
        {
            DependencyService.Register<IIconFontProvider, MacIconFontProvider>();
            DependencyService.Register<INetworkInterfacesService, MacNetworkInterfacesService>();
            DependencyService.Register<ISystemSoundService, MacSystemSoundService>();

            // register all WindowService subtypes
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
                             .Where(t => typeof(WindowService).IsAssignableFrom(t))
                             .Distinct())
            {
                if (type == typeof(WindowService))
                    continue;

                var method = typeof(DependencyService).GetMethod("Register", 1,
                                                                 BindingFlags.Static | BindingFlags.Public, null,
                                                                 CallingConventions.Standard, new Type[] { }, null);
                method.MakeGenericMethod(type).Invoke(null, new object[] { });
            }
        }


        public override void DidFinishLaunching(NSNotification notification)
        {
            Forms.Init();

            _MainWindow = DependencyService.Get<MainWindowService>().OpenWindow();

            LoadApplication(new App());

            base.DidFinishLaunching(notification);
        }

        public override void WillTerminate(NSNotification notification) { }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
            => true;
    }
}
