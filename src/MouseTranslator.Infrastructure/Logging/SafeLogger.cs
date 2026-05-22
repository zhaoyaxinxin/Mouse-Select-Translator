using System.Diagnostics;

namespace MouseTranslator.Infrastructure.Logging;

public sealed class SafeLogger
{
    public void Info(string message)
    {
        Trace.WriteLine($"[INFO] {message}");
    }

    public void Warn(string message)
    {
        Trace.WriteLine($"[WARN] {message}");
    }

    public void Error(string message)
    {
        Trace.WriteLine($"[ERROR] {message}");
    }
}
