using System;

//using AltNetworkUtility.macOS.PrivilegedHelper.XpcProtocol;

//using Foundation;

[assembly: ObjCRuntime.LinkWith(LinkerFlags = "-L/Users/chucker/.nuget/packages/microsoft.netcore.app.runtime.osx-x64/6.0.1/runtimes/osx-x64/native -lSystem.Globalization.Native -L/Users/chucker/.nuget/packages/microsoft.netcore.app.runtime.osx-x64/6.0.1/runtimes/osx-x64/native -lSystem.IO.Compression.Native -L/Users/chucker/.nuget/packages/microsoft.netcore.app.runtime.osx-x64/6.0.1/runtimes/osx-x64/native -lSystem.Native -L/Users/chucker/.nuget/packages/microsoft.netcore.app.runtime.osx-x64/6.0.1/runtimes/osx-x64/native -lSystem.Net.Security.Native -L/Users/chucker/.nuget/packages/microsoft.netcore.app.runtime.osx-x64/6.0.1/runtimes/osx-x64/native -lSystem.Security.Cryptography.Native.Apple -L/Users/chucker/.nuget/packages/microsoft.netcore.app.runtime.osx-x64/6.0.1/runtimes/osx-x64/native -lSystem.Security.Cryptography.Native.OpenSsl -L/Users/chucker/.nuget/packages/microsoft.netcore.app.runtime.osx-x64/6.0.1/runtimes/osx-x64/native -lclrjit -L/Users/chucker/.nuget/packages/microsoft.netcore.app.runtime.osx-x64/6.0.1/runtimes/osx-x64/native -lcoreclr -L/Users/chucker/.nuget/packages/microsoft.netcore.app.runtime.osx-x64/6.0.1/runtimes/osx-x64/native -ldbgshim -L/Users/chucker/.nuget/packages/microsoft.netcore.app.runtime.osx-x64/6.0.1/runtimes/osx-x64/native -lhostfxr -L/Users/chucker/.nuget/packages/microsoft.netcore.app.runtime.osx-x64/6.0.1/runtimes/osx-x64/native -lhostpolicy -L/Users/chucker/.nuget/packages/microsoft.netcore.app.runtime.osx-x64/6.0.1/runtimes/osx-x64/native -lmscordaccore -L/Users/chucker/.nuget/packages/microsoft.netcore.app.runtime.osx-x64/6.0.1/runtimes/osx-x64/native -lmscordbi")]

namespace AltNetworkUtility.macOS.PrivilegedHelper
{
    //[Register("XpcServiceMain")]
    public class Program// : NSObject, INSXpcListenerDelegate, IXpcProtocol
    {
        static void Main(string[] args)
        {
            Console.WriteLine("hello world");
            //AppKit.NSApplication.Init();

            //using (Program program = new Program())
            //    program.Run();
        }

        //void Run()
        //{
        //    NSXpcListener listener = NSXpcListener.ServiceListener;
        //    listener.Delegate = this;
        //    listener.Resume();
        //    // Resume() does not return.
        //}

        //[Export("listener:shouldAcceptNewConnection:")]
        //public bool ShouldAcceptConnection(NSXpcListener listener, NSXpcConnection newConnection)
        //{
        //    newConnection.ExportedInterface = NSXpcInterface.Create(typeof(IXpcProtocol));
        //    newConnection.ExportedObject = this;
        //    newConnection.Resume();
        //    return true;
        //}

        //[Export("getHelloString:returnBlock:")]
        //public void GetHelloString(NSString toWhom, GetHelloStringReturnBlock returnBlock)
        //{
        //    returnBlock(new NSString($"Hello, {toWhom}!"));
        //}
    }
}
