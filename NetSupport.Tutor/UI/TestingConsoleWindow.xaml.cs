using System.IO;
using System.Windows;
using Microsoft.Win32;
using NetSupport.Shared.Models;
using NetSupport.Shared.Services;
using NetSupport.Tutor.Services;

namespace NetSupport.Tutor.UI;

public partial class TestingConsoleWindow : Window
{
    private List<ExamQuestion>? _loadedExam;
    private readonly StudentHelloPayload _targetStudent;
    private readonly TcpCommandSender _commandSender;

    public TestingConsoleWindow(StudentHelloPayload targetStudent, TcpCommandSender commandSender)
    {
        InitializeComponent();
        _targetStudent = targetStudent;
        _commandSender = commandSender;

        bool isArabic = TranslationService.IsArabic;
        this.FlowDirection = isArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        
        Title = isArabic ? $"وحدة التحكم - إرسال إلى {_targetStudent.Name}" : $"Testing Console - Sending to {_targetStudent.Name}";
        
        BtnBrowse.Content = TranslationService.Translate("Browse CSV", isArabic);
        BtnPush.Content = TranslationService.Translate("Push Exam to Selected", isArabic);
        TxtFileName.Text = TranslationService.Translate("No file selected", isArabic);
    }

    private void BtnBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var parser = new CsvParser();
                _loadedExam = parser.ParseCsv(dialog.FileName);
                TxtFileName.Text = $"{Path.GetFileName(dialog.FileName)} ({_loadedExam.Count} questions)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load CSV: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void BtnPush_Click(object sender, RoutedEventArgs e)
    {
        if (_loadedExam == null || _loadedExam.Count == 0)
        {
            MessageBox.Show("Please load a valid exam CSV first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(TxtDuration.Text, out int duration))
        {
            MessageBox.Show("Duration must be a number.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var payload = new PushExamPayload
        {
            Questions = _loadedExam,
            DurationMinutes = duration
        };

        // Send the payload to the specific student
        await _commandSender.SendCommandAsync(_targetStudent.Ip, "PUSH_EXAM", payload, _targetStudent.TcpPort);

        MessageBox.Show("Exam pushed successfully! Waiting for student to login...", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        this.Close();
    }
}
