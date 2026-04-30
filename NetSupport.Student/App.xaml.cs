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
    private ExamLoginWindow? _loginWindow;
    private UI.ExamWindow? _examWindow;
    private Shared.Models.PushExamPayload? _currentExam;

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
                    _currentExam = System.Text.Json.JsonSerializer.Deserialize<Shared.Models.PushExamPayload>(e.Message.Payload.GetRawText());
                    var senderSvc = new TcpUpdateSender(e.TutorIp);
                    _loginWindow = new ExamLoginWindow(senderSvc);
                    _loginWindow.Show();
                    break;
                case "START_EXAM":
                    if (_currentExam != null)
                    {
                        _loginWindow?.Close();
                        _loginWindow = null;
                        
                        var examSender = new TcpUpdateSender(e.TutorIp);
                        _examWindow = new UI.ExamWindow(_currentExam, examSender);
                        _examWindow.Show();
                    }
                    break;
                case "STOP_EXAM":
                    if (_examWindow != null)
                    {
                        _examWindow.ForceSubmit();
                    }
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
