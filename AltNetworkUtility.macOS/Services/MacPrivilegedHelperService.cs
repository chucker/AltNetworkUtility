using System;
using System.Runtime.InteropServices;

using AltNetworkUtility.Services;

using Foundation;

using ObjCRuntime;

using Security;

namespace AltNetworkUtility.macOS.Services
{
    public class MacPrivilegedHelperService : IPrivilegedHelperService
    {
        private class NativeMethods
        {
            //[DllImport("/System/Library/Frameworks/ServiceManagement.framework/ServiceManagement")]
            //public static extern bool SMJobBless(IntPtr domain, IntPtr executableLabel, IntPtr authRef, out IntPtr error);

            [Deprecated(PlatformName.MacOSX, 10, 7)]
            [DllImport(Constants.SecurityLibrary)]
            public extern static int /* OSStatus = int */ AuthorizationExecuteWithPrivileges(IntPtr handle,
                string pathToTool, AuthorizationFlags flags, string[] args, ref IntPtr FILEPtr);

            [DllImport("libSystem.dylib")]
            public extern static int _fileno(IntPtr file);

            [DllImport("libSystem.dylib")]
            public extern static int fcntl(int fileNo, int cmd, params int[] arg);
        }

        [Deprecated(PlatformName.MacOSX, 10, 7)]
        private int ExecuteWithPrivileges(Authorization authorization, string pathToTool, AuthorizationFlags flags, string[] args,
            ref IntPtr file)
        {
            return NativeMethods.AuthorizationExecuteWithPrivileges(authorization.Handle, pathToTool, flags, args, ref file);
        }

        public bool TryInstallHelper()
        {
            using (var auth = Security.Authorization.Create(Security.AuthorizationFlags.Defaults))
            {
                IntPtr file = IntPtr.Zero;
                var result = ExecuteWithPrivileges(auth, "/usr/bin/top", AuthorizationFlags.Defaults, new string[] { }, ref file);

                var fileno = NativeMethods._fileno(file);
                var pid = Mono.Unix.Native.Syscall.fcntl(fileno, Mono.Unix.Native.FcntlCommand.F_GETOWN);

                //var pid = NativeMethods.fcntl(fileno, 9);

                //using (var domain = new NSString(""))
                //using (var executableLabel = new NSString("me.chucker.AltNetworkUtility.macOS.PrivilegedHelper"))
                //{
                //    auth.ExecuteWithPrivileges(, Security.AuthorizationFlags,,)
                //    var result = NativeMethods.SMJobBless(domain.Handle, executableLabel.Handle, auth.Handle, out var _error);

                //    var error = Runtime.GetNSObject<NSError>(_error);
                //}
            }

            return false;
        }
    }
}
