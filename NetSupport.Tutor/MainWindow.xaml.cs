using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using NetSupport.Shared.Models;
using NetSupport.Shared.Services;
using NetSupport.Tutor.Services;

namespace NetSupport.Tutor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private UdpListener _listener;
    private TcpUpdateListener _updateListener;
    private ObservableCollection<StudentHelloPayload> _discoveredStudents;
    private TcpCommandSender _commandSender;

    public MainWindow()
    {
        InitializeComponent();
        
        _discoveredStudents = new ObservableCollection<StudentHelloPayload>();
        StudentsDataGrid.ItemsSource = _discoveredStudents;

        _listener = new UdpListener();
        _listener.StudentDiscovered += OnStudentDiscovered;
        _listener.StartListening();

        _updateListener = new TcpUpdateListener();
        _updateListener.StudentReadyReceived += OnStudentReadyReceived;
        _updateListener.AnswerUpdateReceived += OnAnswerUpdateReceived;
        _updateListener.ExamResultReceived += OnExamResultReceived;
        _updateListener.RaiseHandReceived += OnRaiseHandReceived;
        _updateListener.StartListening();

        _commandSender = new TcpCommandSender();
    }

    private void OnStudentDiscovered(object? sender, StudentHelloPayload student)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Use the unique instance Id (not IP) to allow multiple instances on one machine
            var existing = _discoveredStudents.FirstOrDefault(s => s.Id == student.Id);
            if (existing == null)
            {
                _discoveredStudents.Add(student);
            }
        });
    }

    private void OnStudentReadyReceived(object? sender, StudentReadyPayload payload)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var existing = _discoveredStudents.FirstOrDefault(s => s.Ip == payload.Ip);
            if (existing != null)
            {
                existing.IsReady = true;
                if (!string.IsNullOrWhiteSpace(payload.StudentName))
                {
                    existing.Name = payload.StudentName; // Update with real name
                }
            }
        });
    }

    private void OnAnswerUpdateReceived(object? sender, AnswerUpdatePayload payload)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var existing = _discoveredStudents.FirstOrDefault(s => s.Ip == payload.Ip);
            if (existing != null)
            {
                existing.Score = payload.ScoreString;
            }
        });
    }

    private void OnExamResultReceived(object? sender, ExamResultPayload payload)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var existing = _discoveredStudents.FirstOrDefault(s => s.Ip == payload.Ip);
            if (existing != null)
            {
                existing.Score = $"FINAL: {payload.FinalScore}";
                existing.IsReady = false; // Exam is over
                existing.DetailedResults = payload.DetailedAnswers; // Save detailed data for PDF
            }
        });
    }

    private void OnRaiseHandReceived(object? sender, RaiseHandPayload payload)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var existing = _discoveredStudents.FirstOrDefault(s => s.Ip == payload.Ip);
            if (existing != null)
            {
                existing.IsRaisingHand = payload.IsRaising;
            }
        });
    }

    private void ClearHelpBtn_Click(object sender, RoutedEventArgs e)
    {
        if (StudentsDataGrid.SelectedItem is StudentHelloPayload student)
        {
            student.IsRaisingHand = false;
        }
        else
        {
            MessageBox.Show("Please select a student from the list first.", "No Student Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void LockBtn_Click(object sender, RoutedEventArgs e)
    {
        if (StudentsDataGrid.SelectedItem is StudentHelloPayload student)
        {
            await _commandSender.SendCommandAsync(student.Ip, "LOCK", null, student.TcpPort);
            student.IsLocked = true; // Update the UI flag
        }
        else
        {
            MessageBox.Show("Please select a student from the list first.", "No Student Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void UnlockBtn_Click(object sender, RoutedEventArgs e)
    {
        if (StudentsDataGrid.SelectedItem is StudentHelloPayload student)
        {
            await _commandSender.SendCommandAsync(student.Ip, "UNLOCK", null, student.TcpPort);
            student.IsLocked = false; // Update the UI flag
        }
        else
        {
            MessageBox.Show("Please select a student from the list first.", "No Student Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void TestingConsoleBtn_Click(object sender, RoutedEventArgs e)
    {
        if (StudentsDataGrid.SelectedItem is StudentHelloPayload student)
        {
            var console = new UI.TestingConsoleWindow(student, _commandSender);
            console.Owner = this;
            console.Show();
        }
        else
        {
            MessageBox.Show("Please select a student from the list first.", "No Student Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void StartExamBtn_Click(object sender, RoutedEventArgs e)
    {
        if (StudentsDataGrid.SelectedItem is StudentHelloPayload student)
        {
            if (!student.IsReady)
            {
                MessageBox.Show("This student has not joined an exam yet.\n\nPlease click 'Open Testing Console' to push an exam to this student first, then wait for them to log in.", "Exam Not Ready", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            await _commandSender.SendCommandAsync(student.Ip, "START_EXAM", null, student.TcpPort);
            student.Score = "Started...";
            MessageBox.Show($"Exam started for {student.Name}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("Please select a student from the list first.", "No Student Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void StopExamBtn_Click(object sender, RoutedEventArgs e)
    {
        if (StudentsDataGrid.SelectedItem is StudentHelloPayload student)
        {
            if (!student.IsReady && string.IsNullOrEmpty(student.Score))
            {
                MessageBox.Show("This student is not currently taking an exam.", "No Active Exam", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Are you sure you want to stop the exam for {student.Name}? This will force them to submit.", "Confirm Stop", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                await _commandSender.SendCommandAsync(student.Ip, "STOP_EXAM", null, student.TcpPort);
                MessageBox.Show("Stop command sent.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        else
        {
            MessageBox.Show("Please select a student from the list first.", "No Student Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void GenerateReportBtn_Click(object sender, RoutedEventArgs e)
    {
        var finishedStudents = _discoveredStudents.Where(s => s.DetailedResults != null && s.DetailedResults.Count > 0).ToList();

        if (finishedStudents.Count == 0)
        {
            MessageBox.Show("No students have submitted an exam yet! Please wait for at least one student to finish before generating a report.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            bool isArabic = LanguageToggle.SelectedItem != null && ((ComboBoxItem)LanguageToggle.SelectedItem).Tag.ToString() == "ar";
            var generator = new PdfReportGenerator();
            string filePath = generator.GenerateReport(finishedStudents, isArabic); // Only pass finished students

            var result = MessageBox.Show($"Report successfully generated!\n\nSaved to:\n{filePath}\n\nDo you want to open the folder containing the report?", "Success", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to generate report: {ex.Message}\nMake sure you added the QuestPDF NuGet package!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LanguageToggle_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        
        bool isArabic = ((ComboBoxItem)LanguageToggle.SelectedItem).Tag.ToString() == "ar";
        
        // Flip RTL
        this.FlowDirection = isArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        
        // Translate UI
        LblTitle.Text = TranslationService.Translate("Discovered Students:", isArabic);
        LblLanguage.Content = TranslationService.Translate("Language:", isArabic);
        LockBtn.Content = TranslationService.Translate("Lock Selected", isArabic);
        UnlockBtn.Content = TranslationService.Translate("Unlock Selected", isArabic);
        TestingConsoleBtn.Content = TranslationService.Translate("Open Testing Console", isArabic);
        StartExamBtn.Content = TranslationService.Translate("Start Exam", isArabic);
        StopExamBtn.Content = TranslationService.Translate("Stop Exam", isArabic);
        GenerateReportBtn.Content = TranslationService.Translate("Generate Report", isArabic);

        // Translate DataGrid Headers
        if (StudentsDataGrid.Columns.Count >= 5)
        {
            StudentsDataGrid.Columns[1].Header = TranslationService.Translate("Student Name", isArabic); // Assuming column indexes
            StudentsDataGrid.Columns[2].Header = TranslationService.Translate("IP Address", isArabic);
            StudentsDataGrid.Columns[3].Header = TranslationService.Translate("Status", isArabic);
            StudentsDataGrid.Columns[4].Header = TranslationService.Translate("Score", isArabic);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _listener.Stop();
        _updateListener.Stop();
        base.OnClosed(e);
    }
}