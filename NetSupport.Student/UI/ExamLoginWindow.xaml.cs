using System.Windows;
using System.Windows.Controls;
using NetSupport.Shared.Models;
using NetSupport.Shared.Services;
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

    private async void BtnReady_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtName.Text))
        {
            MessageBox.Show("Please enter your name.");
            return;
        }

        BtnReady.IsEnabled = false;
        BtnReady.Content = "Waiting...";

        var payload = new StudentReadyPayload
        {
            Ip = GetLocalIpAddress(),
            StudentName = TxtName.Text.Trim()
        };

        await _updateSender.SendUpdateAsync("STUDENT_READY", payload);
        
        bool isArabic = LanguageToggle.SelectedItem != null && ((ComboBoxItem)LanguageToggle.SelectedItem).Tag.ToString() == "ar";
        LblPrompt.Text = TranslationService.Translate("Waiting for instructor to start the exam...", isArabic);
    }

    private void LanguageToggle_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        
        bool isArabic = ((ComboBoxItem)LanguageToggle.SelectedItem).Tag.ToString() == "ar";
        TranslationService.IsArabic = isArabic;
        
        this.FlowDirection = isArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        
        LblLanguage.Content = TranslationService.Translate("Language:", isArabic);
        LblWelcome.Text = TranslationService.Translate("Welcome to the Exam!", isArabic);
        LblPrompt.Text = TranslationService.Translate("Please enter your full name to begin:", isArabic);
        BtnReady.Content = TranslationService.Translate("I am Ready", isArabic);
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
