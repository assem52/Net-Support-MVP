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
        _commandListener.CommandReceived += OnCommandReceived;
        _commandListener.StartListening();
    }

    private void OnCommandReceived(object? sender, CommandReceivedEventArgs e)
    {
        Current.Dispatcher.Invoke(() =>
        {
            switch (e.Message.Type)
            {
                case "LOCK":
                    if (_lockWindow == null)
                    {
                        _lockWindow = new LockScreenWindow();
                        _lockWindow.Show();
                    }
                    break;
                case "UNLOCK":
                    if (_lockWindow != null)
                    {
                        _lockWindow.Close();
                        _lockWindow = null;
                    }
                    break;
                case "PUSH_EXAM":
                    // Provide the Tutor IP back to the login window so it knows who to talk to
                    var senderSvc = new TcpUpdateSender(e.TutorIp);
                    var loginWin = new ExamLoginWindow(senderSvc);
                    loginWin.Show();
                    break;
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
