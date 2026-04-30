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
            Ip = "" // Connection inherently gives tutor the IP
        };

        await _updateSender.SendUpdateAsync("STUDENT_READY", payload);

        BtnSubmit.Visibility = Visibility.Collapsed;
        TxtName.Visibility = Visibility.Collapsed;
        TxtWaiting.Visibility = Visibility.Visible;
    }

    protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.System && e.SystemKey == System.Windows.Input.Key.F4)
        {
            e.Handled = true; // Block Alt-F4
        }
        base.OnPreviewKeyDown(e);
    }
}
