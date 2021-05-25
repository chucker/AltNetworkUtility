using System;
using System.Runtime.InteropServices;

using Foundation;

using ObjCRuntime;

#nullable enable

namespace AltNetworkUtility.macOS.Models
{
    public class SCNetworkInterface
    {
        private class NativeMethods
        {
            /* 
             * Unused. MAYBE actually implement some support for this. We can use this:
             * 
             * var lastError = NativeMethods.SCCopyLastError();
             * var cfException = CoreFoundation.CFException.FromCFError(lastError);
             */
            //[DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            //public static extern IntPtr SCCopyLastError();

            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern IntPtr SCDynamicStoreCreate(IntPtr allocator, IntPtr name, IntPtr callout, IntPtr context);

            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern IntPtr SCDynamicStoreCopyValue(IntPtr store, IntPtr key);

            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern IntPtr SCNetworkInterfaceGetBSDName(IntPtr scNetworkInterfaceRef);

            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern IntPtr SCNetworkInterfaceGetLocalizedDisplayName(IntPtr scNetworkInterfaceRef);

            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern IntPtr SCNetworkInterfaceGetInterfaceType(IntPtr scNetworkInterfaceRef);
        }

        public IntPtr SCNetworkInterfaceRef { get; }

        public NSString BsdName { get; }
        public NSString InterfaceType { get; }
        public bool IsUp { get; }
        public NSString LocalizedDisplayName { get; }

        private NSDictionary GetDynamicStoreDictionaryValue(string path)
        {
            IntPtr allocator = default;
            using (var dynamicStoreName = new NSString("test"))
            using (var _path = new NSString(path))
            {
                var dynSt = NativeMethods.SCDynamicStoreCreate(allocator, dynamicStoreName.GetHandle(), default, default);

                var value = NativeMethods.SCDynamicStoreCopyValue(dynSt, _path.GetHandle());

                return Runtime.GetNSObject<NSDictionary>(value);
            }
        }

        public SCNetworkInterface(IntPtr scNetworkInterfaceRef)
        {
            SCNetworkInterfaceRef = scNetworkInterfaceRef;

            IntPtr bsdName = NativeMethods.SCNetworkInterfaceGetBSDName(scNetworkInterfaceRef);
            BsdName = Runtime.GetNSObject<NSString>(bsdName);

            IntPtr localizedDisplayName = NativeMethods.SCNetworkInterfaceGetLocalizedDisplayName(scNetworkInterfaceRef);
            LocalizedDisplayName = Runtime.GetNSObject<NSString>(localizedDisplayName);

            IntPtr interfaceType = NativeMethods.SCNetworkInterfaceGetInterfaceType(scNetworkInterfaceRef);
            InterfaceType = Runtime.GetNSObject<NSString>(interfaceType);

            if (GetDynamicStoreDictionaryValue($"State:/Network/Interface/{BsdName}/Link") is NSDictionary dict &&
                dict.TryGetValue(new NSString("Active"), out var isActive) &&
                isActive is NSNumber number)
            {
                IsUp = number.BoolValue;
            }
        }
    }
}
