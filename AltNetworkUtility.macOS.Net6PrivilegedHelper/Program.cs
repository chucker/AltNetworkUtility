using System;

using AltNetworkUtility.macOS.PrivilegedHelper.XpcProtocol;

using Foundation;

namespace AltNetworkUtility.macOS.PrivilegedHelper
{
    [Register("XpcServiceMain")]
    public class Program : NSObject, INSXpcListenerDelegate, IXpcProtocol
    {
        static void Main(string[] args)
        {
            AppKit.NSApplication.Init();

            using (Program program = new Program())
                program.Run();
        }

        void Run()
        {
            NSXpcListener listener = NSXpcListener.ServiceListener;
            listener.Delegate = this;
            listener.Resume();
            // Resume() does not return.
        }

        [Export("listener:shouldAcceptNewConnection:")]
        public bool ShouldAcceptConnection(NSXpcListener listener, NSXpcConnection newConnection)
        {
            newConnection.ExportedInterface = NSXpcInterface.Create(typeof(IXpcProtocol));
            newConnection.ExportedObject = this;
            newConnection.Resume();
            return true;
        }

        [Export("getHelloString:returnBlock:")]
        public void GetHelloString(NSString toWhom, GetHelloStringReturnBlock returnBlock)
        {
            returnBlock(new NSString($"Hello, {toWhom}!"));
        }
    }
}
