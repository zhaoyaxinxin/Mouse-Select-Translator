using System.Runtime.InteropServices;

namespace MouseTranslator.Infrastructure.Win32;

public sealed class SendInputService : ICopyCommandSender
{
    public Task SendCtrlCAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var inputs = new[]
        {
            CreateKeyInput(NativeMethods.VK_CONTROL, 0),
            CreateKeyInput(NativeMethods.VK_C, 0),
            CreateKeyInput(NativeMethods.VK_C, NativeMethods.KEYEVENTF_KEYUP),
            CreateKeyInput(NativeMethods.VK_CONTROL, NativeMethods.KEYEVENTF_KEYUP),
        };

        var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException("Failed to send Ctrl+C.");
        }

        return Task.CompletedTask;
    }

    private static NativeMethods.INPUT CreateKeyInput(int virtualKey, uint flags)
    {
        return new NativeMethods.INPUT
        {
            Type = 1,
            U = new NativeMethods.InputUnion
            {
                Ki = new NativeMethods.KEYBDINPUT
                {
                    Vk = (ushort)virtualKey,
                    Flags = flags,
                },
            },
        };
    }
}
