using System.Configuration;
using System.Data;
using System.Windows;
using NetSupport.Student.Services;

namespace NetSupport.Student;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private UdpBroadcaster? _broadcaster;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Start our background broadcaster
        _broadcaster = new UdpBroadcaster();
        
        // We use Task.Run to run it on a background thread so it doesn't freeze the UI
        Task.Run(() => _broadcaster.StartBroadcastingAsync(Environment.MachineName, "EvalRoom"));
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Cleanly stop the broadcaster when the app closes
        _broadcaster?.Stop();
        base.OnExit(e);
    }
}
