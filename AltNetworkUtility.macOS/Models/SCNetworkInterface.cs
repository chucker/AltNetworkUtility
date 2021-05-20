using System;
using System.Runtime.InteropServices;

using AltNetworkUtility.macOS.Services;
using AltNetworkUtility.Services;

using CoreFoundation;

using Foundation;

using ObjCRuntime;

#nullable enable

namespace AltNetworkUtility.macOS.Models
{
    public class SCNetworkInterface : ViewModelBase
    {
        readonly Serilog.ILogger Log = Serilog.Log.ForContext<SCNetworkInterface>();

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

            public delegate void SCDynamicStoreCallBack(IntPtr store, IntPtr changedKeys, IntPtr info);

            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern IntPtr SCDynamicStoreCreate(IntPtr allocator, IntPtr name,
                                                             SCDynamicStoreCallBack? callout, IntPtr context);

            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern IntPtr SCDynamicStoreCopyValue(IntPtr store, IntPtr key);

            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern bool SCDynamicStoreSetNotificationKeys(IntPtr store, IntPtr keys, IntPtr patterns);

            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern bool SCDynamicStoreSetDispatchQueue(IntPtr dynSt, IntPtr handle);

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

        private bool _IsUp;
        public bool IsUp
        {
            get => _IsUp;
            set => SetProperty(ref _IsUp, value);
        }

        public NSString LocalizedDisplayName { get; }

        private static NSDictionary GetDynamicStoreDictionaryValue(string path)
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

            IntPtr scInfo;

            scInfo = NativeMethods.SCNetworkInterfaceGetBSDName(scNetworkInterfaceRef);
            BsdName = Runtime.GetNSObject<NSString>(scInfo);

            scInfo = NativeMethods.SCNetworkInterfaceGetLocalizedDisplayName(scNetworkInterfaceRef);
            LocalizedDisplayName = Runtime.GetNSObject<NSString>(scInfo);

            scInfo = NativeMethods.SCNetworkInterfaceGetInterfaceType(scNetworkInterfaceRef);
            InterfaceType = Runtime.GetNSObject<NSString>(scInfo);

            IsUp = GetIsUp(BsdName);

            IntPtr allocator = default;
            var dynamicStoreName = new NSString("testWithCallback");
            var callback = new NativeMethods.SCDynamicStoreCallBack((store, changedKeys, info) =>
            {
                // FIXME
                string bsdName = "en0";

                var svc = Xamarin.Forms.DependencyService.Get<INetworkInterfacesService>() as MacNetworkInterfacesService;

                if (svc == null) return;

                if (!svc.SCNetworkInterfaces.TryGetValue(bsdName, out var networkInterface))
                    Log.Warning($"Couldn't find SCNetworkInterface for BSD name {bsdName} from callback.");
                else
                    networkInterface.IsUp = GetIsUp(bsdName);
            });
            var dynSt = NativeMethods.SCDynamicStoreCreate(allocator, dynamicStoreName.GetHandle(), callback, default);

            if (NativeMethods.SCDynamicStoreSetNotificationKeys(dynSt, NSArray.FromStrings("State:/Network/Global/IPv4").Handle, default))
            {
                bool success = NativeMethods.SCDynamicStoreSetDispatchQueue(dynSt, DispatchQueue.MainQueue.Handle);
            }
        }

        private static bool GetIsUp(string bsdName)
        {
            if (GetDynamicStoreDictionaryValue($"State:/Network/Interface/{bsdName}/Link") is NSDictionary dict)
            {
                if (dict.TryGetValue(new NSString("Active"), out var isActive))
                {
                    if (isActive is NSNumber number)
                    {
                        return number.BoolValue;
                    }
                }
            }

            return false;
        }
    }
}
