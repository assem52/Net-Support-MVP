using System.Configuration;
using System.Data;
using System.Windows;
using NetSupport.Student.Services;
using NetSupport.Student.UI;

namespace NetSupport.Student;

public partial class App : Application
{
    private UdpBroadcaster? _broadcaster;
    private TcpCommandListener? _commandListener;
    private LockScreenWindow? _lockWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. Start UDP Broadcaster (shouts presence)
        _broadcaster = new UdpBroadcaster();
        Task.Run(() => _broadcaster.StartBroadcastingAsync(Environment.MachineName, "EvalRoom"));

        // 2. Start TCP Command Listener (waits for LOCK commands)
        _commandListener = new TcpCommandListener();
        _commandListener.LockCommandReceived += OnLockCommandReceived;
        _commandListener.UnlockCommandReceived += OnUnlockCommandReceived;
        _commandListener.StartListening();
    }

    private void OnLockCommandReceived(object? sender, EventArgs e)
    {
        // UI code must run on the UI dispatcher
        Current.Dispatcher.Invoke(() =>
        {
            if (_lockWindow == null)
            {
                _lockWindow = new LockScreenWindow();
                _lockWindow.Show();
            }
        });
    }

    private void OnUnlockCommandReceived(object? sender, EventArgs e)
    {
        Current.Dispatcher.Invoke(() =>
        {
            if (_lockWindow != null)
            {
                _lockWindow.Close();
                _lockWindow = null;
            }
        });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _broadcaster?.Stop();
        _commandListener?.Stop();
        base.OnExit(e);
    }
}
