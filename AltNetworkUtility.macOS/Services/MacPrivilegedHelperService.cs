using System;
using System.Runtime.InteropServices;

using AltNetworkUtility.Services;

using Foundation;

using ObjCRuntime;

namespace AltNetworkUtility.macOS.Services
{
    public class MacPrivilegedHelperService : IPrivilegedHelperService
    {
        private class NativeMethods
        {
            [DllImport("/System/Library/Frameworks/ServiceManagement.framework/ServiceManagement")]
            public static extern bool SMJobBless(IntPtr domain, IntPtr executableLabel, IntPtr authRef, out IntPtr error);
        }

        public bool TryInstallHelper()
        {
            using (var auth = Security.Authorization.Create(Security.AuthorizationFlags.Defaults))
            {
                using (var domain = new NSString(""))
                using (var executableLabel = new NSString("me.chucker.AltNetworkUtility.macOS.PrivilegedHelper"))
                {
                    var result = NativeMethods.SMJobBless(domain.Handle, executableLabel.Handle, auth.Handle, out var _error);

                    var error = Runtime.GetNSObject<NSError>(_error);
                }
            }

            return false;
        }
    }
}
