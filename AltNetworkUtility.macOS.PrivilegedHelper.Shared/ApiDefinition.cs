using Foundation;

using ObjCRuntime;

namespace AltNetworkUtility.macOS.PrivilegedHelper.XpcProtocol
{
    delegate void GetHelloStringReturnBlock(NSString message);

    [Preserve, XpcInterface]
    [Protocol(Name = "XamarinXpcProtocol")]
    [BaseType(typeof(NSObject))]
    interface XpcProtocol
    {
        [Export("getHelloString:returnBlock:")]
        void GetHelloString(NSString toWhom, [BlockCallback] GetHelloStringReturnBlock returnBlock);
    }
}
