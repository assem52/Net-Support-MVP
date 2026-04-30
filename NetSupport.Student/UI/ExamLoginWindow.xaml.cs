using System.Windows;
using NetSupport.Shared.Models;
using NetSupport.Student.Services;

namespace NetSupport.Student.UI;

public partial class ExamLoginWindow : Window
{
    private readonly TcpUpdateSender _updateSender;

    public ExamLoginWindow(TcpUpdateSender updateSender)
    {
        InitializeComponent();
        _updateSender = updateSender;
    }

    private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtName.Text))
        {
            MessageBox.Show("Please enter your name.");
            return;
        }

        var payload = new StudentReadyPayload
        {
            StudentName = TxtName.Text,
            Ip = GetLocalIpAddress() // Explicitly send the exact same IP we used for UDP discovery
        };

        await _updateSender.SendUpdateAsync("STUDENT_READY", payload);

        BtnSubmit.Visibility = Visibility.Collapsed;
        TxtName.Visibility = Visibility.Collapsed;
        TxtWaiting.Visibility = Visibility.Visible;
    }

    private string GetLocalIpAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }

    protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        // EMERGENCY BACKDOOR FOR TESTING: Ctrl + Shift + Q closes the app
        if (e.Key == System.Windows.Input.Key.Q && 
            System.Windows.Input.Keyboard.Modifiers == (System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift))
        {
            Application.Current.Shutdown();
            return;
        }

        if (e.Key == System.Windows.Input.Key.System && e.SystemKey == System.Windows.Input.Key.F4)
        {
            e.Handled = true; // Block Alt-F4
        }
        base.OnPreviewKeyDown(e);
    }
}
