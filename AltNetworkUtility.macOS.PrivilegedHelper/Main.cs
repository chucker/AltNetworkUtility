using AppKit;

namespace AltNetworkUtility.macOS.PrivilegedHelper
{
    static class MainClass
    {
        static void Main(string[] args)
        {
            NSApplication.Init();
            NSApplication.Main(args);
        }
    }
}
