using System;
using System.Collections.Generic;
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
            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern IntPtr SCCopyLastError();

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

            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern IntPtr SCNetworkInterfaceGetConfiguration(IntPtr scNetworkInterfaceRef);

            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern IntPtr SCNetworkInterfaceGetExtendedConfiguration(IntPtr scNetworkInterfaceRef);
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
            //System.Net.NetworkInformation.OperationalStatus
            IntPtr localizedDisplayName = NativeMethods.SCNetworkInterfaceGetLocalizedDisplayName(scNetworkInterfaceRef);
            LocalizedDisplayName = Runtime.GetNSObject<NSString>(localizedDisplayName);

            IntPtr interfaceType = NativeMethods.SCNetworkInterfaceGetInterfaceType(scNetworkInterfaceRef);
            InterfaceType = Runtime.GetNSObject<NSString>(interfaceType);

            var serviceOrder = GetDynamicStoreDictionaryValue("Setup:/Network/Global/IPv4")["ServiceOrder"] as NSArray;

            //var bsdInterfaceNameToSCServiceName = new Dictionary<string, string>();

            //for (nuint i = 0; i < serviceOrder!.Count; i++)
            //{
            //    var item = serviceOrder.GetItem<NSString>(i).ToString();

            //    var service = GetDynamicStoreDictionaryValue($"Setup:/Network/Service/{item}/Interface");

            //    if (service.TryGetValue(new NSString("DeviceName"), out var deviceName))
            //        bsdInterfaceNameToSCServiceName[((NSString)deviceName).ToString()] = item;
            //}

            try
            {
            if (GetDynamicStoreDictionaryValue($"State:/Network/Interface/{BsdName}/Link") is NSDictionary dict &&
                dict.TryGetValue(new NSString("Active"), out var isActive))
            {
                if (isActive is NSNumber number)
                {
                    IsUp = number.BoolValue;
                }
            }
            }
            catch(Exception ex)
            {

            }

            //GetDynamicStoreDictionary("Setup:/Network/Global/IPv4").ToString()  "{\n    ServiceOrder =     (\n        \"20C6B381-B198-432A-808F-5670CCB6E619\",\n        \"C12DB364-EDD6-429E-8A50-23CB85EC402F\",\n        \"88C907EC-EE16-497E-BC7F-33D71955110C\",\n        \"97537565-6B6C-4E47-895B-124134E43CBE\",\n        \"96BA04CE-EC43-4C16-A958-AA05DC04…"  string
            //NSString name = new NSString("test");

            //using (var dynamicStoreName = new NSString("test"))
            ////using (var path = new NSString("Setup:/Network/Global/IPv4"))
            ////using (var path = new NSString("Setup:/Network/Interface/en0/Link"))
            //{
            //    var dynSt = NativeMethods.SCDynamicStoreCreate(allocator, dynamicStoreName.GetHandle(), default, default);

            //    var value = NativeMethods.SCDynamicStoreCopyValue(dynSt, path.GetHandle());
            //}
            //var dynSt = NativeMethods.SCDynamicStoreCreate(allocator, name.GetHandle(), default, default);

            //var value = NativeMethods.SCDynamicStoreCopyValue(dynSt, new NSString("Setup:/Network/Global/IPv4").GetHandle());

            //IntPtr configuration = NativeMethods.SCNetworkInterfaceGetConfiguration(scNetworkInterfaceRef);
            //var _configuration = Runtime.GetNSObject<NSDictionary>(configuration);

            //var lastError = NativeMethods.SCCopyLastError();

            //var cfException = CoreFoundation.CFException.FromCFError(lastError);

            //IntPtr extConfiguration = NativeMethods.SCNetworkInterfaceGetExtendedConfiguration(scNetworkInterfaceRef);
            //var _extConfiguration = Runtime.GetNSObject<NSDictionary>(configuration);
        }
    }
}
