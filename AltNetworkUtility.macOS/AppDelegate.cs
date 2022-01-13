using System;
using System.Linq;
using System.Reflection;

using AltNetworkUtility.macOS.Services;
using AltNetworkUtility.macOS.Services.Windows;
using AltNetworkUtility.Repositories.NetworkInterfaceRepository;
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
    public partial class AppDelegate : FormsApplicationDelegate
    {
        private NSWindow? _MainWindow;
        public override NSWindow? MainWindow => _MainWindow;

        public static string AppName => Assembly.GetExecutingAssembly().GetName().Name;

        public AppDelegate()
        {
            DependencyService.Register<IIconFontProvider, MacIconFontProvider>();
            DependencyService.Register<ISystemSoundService, MacSystemSoundService>();

            DependencyService.Register<Repository>();

            DependencyService.Register<IOpenUrlService, MacOpenUrlService>();

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

            InitNetworkInterfaceRepo();

            _MainWindow = DependencyService.Get<MainWindowService>().OpenWindow();

            LoadApplication(new App());

            base.DidFinishLaunching(notification);
        }

        private void InitNetworkInterfaceRepo()
        {
            var repo = DependencyService.Get<Repository>();

            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                                          .Where(t => typeof(IDataSource).IsAssignableFrom(t))
                                          .Distinct())
            {
                if (type == typeof(IDataSource))
                    continue;

                var instance = Activator.CreateInstance(type);
                repo.RegisterDataSource((IDataSource)instance);
            }

            repo.ReloadAll(DataSourceKind.All);
        }

        public override void WillTerminate(NSNotification notification) { }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
            => true;

        partial void ShowAboutBox(NSObject sender)
        {
            DependencyService.Get<AboutBoxWindowService>().OpenWindow();
        }

        partial void CheckForUpdates(NSObject sender)
        {
            var updater = new Sparkle.SUUpdater();
            updater.CheckForUpdates(this);
        }
    }
}
