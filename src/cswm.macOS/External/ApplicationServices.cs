using System.Runtime.InteropServices;

namespace cswm.macOS.External;

public static class ApplicationServices
{
    [DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
    public static extern bool AXIsProcessTrusted();
}