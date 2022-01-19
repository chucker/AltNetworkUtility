using AppKit;

using Serilog;
using Serilog.Sinks.AppleUnifiedLogging;

#nullable enable

namespace AltNetworkUtility.macOS
{
    static class MainClass
    {
        static void Main(string[] args)
        {
            NSApplication.Init();

            Log.Logger = new LoggerConfiguration()
#if DEBUG
               .MinimumLevel.Debug()
#endif
               .WriteTo.AppleUnifiedLogging()
               .CreateLogger();
            
            NSApplication.SharedApplication.Delegate = new AppDelegate();
            NSApplication.Main(args);
        }
    }
}
