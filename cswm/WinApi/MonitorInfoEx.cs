using System.Runtime.InteropServices;

namespace cswm.WinApi;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
public struct MonitorInfoEx
{
    private const int CCHDEVICENAME = 32;

    public uint cbSize;
    public Rect rcMonitor;
    public Rect rcWork;
    public uint dwFlags;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
    public string szDevice;

    public static MonitorInfoEx Create() =>
        new MonitorInfoEx { cbSize = (uint)Marshal.SizeOf<MonitorInfoEx>() };
}