namespace MouseTranslator.Infrastructure.Win32;

public interface ICopyCommandSender
{
    Task SendCtrlCAsync(CancellationToken cancellationToken);
}
