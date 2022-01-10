using System;

using AltNetworkUtility.Services;

using AppKit;

namespace AltNetworkUtility.macOS.Services
{
	public class MacOpenUrlService : IOpenUrlService
	{
        public void Open(Uri uri)
        {
            _ = NSWorkspace.SharedWorkspace.OpenUrl(uri);
        }
    }
}
