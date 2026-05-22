using System.Diagnostics;
using MouseTranslator.Core.Selection;

namespace MouseTranslator.Infrastructure.Win32;

public sealed class ForegroundWindowInfoProvider : IForegroundWindowInfoProvider
{
    public string? GetForegroundProcessName()
    {
        var windowHandle = NativeMethods.GetForegroundWindow();
        if (windowHandle == nint.Zero)
        {
            return null;
        }

        NativeMethods.GetWindowThreadProcessId(windowHandle, out var processId);
        if (processId == 0)
        {
            return null;
        }

        try
        {
            using var process = Process.GetProcessById((int)processId);
            return $"{process.ProcessName}.exe";
        }
        catch
        {
            return null;
        }
    }
}
