﻿using System;
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
            public static extern IntPtr SCNetworkInterfaceGetBSDName(IntPtr scNetworkInterfaceRef);

            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern IntPtr SCNetworkInterfaceGetLocalizedDisplayName(IntPtr scNetworkInterfaceRef);

            [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
            public static extern IntPtr SCNetworkInterfaceGetInterfaceType(IntPtr scNetworkInterfaceRef);
        }

        public IntPtr SCNetworkInterfaceRef { get; }

        public NSString BsdName { get; }
        public NSString InterfaceType { get; }
        public NSString LocalizedDisplayName { get; }

        public SCNetworkInterface(IntPtr scNetworkInterfaceRef)
        {
            SCNetworkInterfaceRef = scNetworkInterfaceRef;

            IntPtr bsdName = NativeMethods.SCNetworkInterfaceGetBSDName(scNetworkInterfaceRef);
            BsdName = Runtime.GetNSObject<NSString>(bsdName);

            IntPtr localizedDisplayName = NativeMethods.SCNetworkInterfaceGetLocalizedDisplayName(scNetworkInterfaceRef);
            LocalizedDisplayName = Runtime.GetNSObject<NSString>(localizedDisplayName);

            IntPtr interfaceType = NativeMethods.SCNetworkInterfaceGetInterfaceType(scNetworkInterfaceRef);
            InterfaceType = Runtime.GetNSObject<NSString>(interfaceType);
        }
    }
}
