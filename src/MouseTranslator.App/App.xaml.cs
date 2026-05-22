namespace MouseTranslator.App;

public partial class App : System.Windows.Application
{
    private ApplicationController? _applicationController;

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
        _applicationController = CompositionRoot.Create(this);
        _applicationController.Start();
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        _applicationController?.Dispose();
        base.OnExit(e);
    }
}
