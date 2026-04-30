using System.Collections.ObjectModel;
using System.Windows;
using NetSupport.Shared.Models;
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
        _updateListener.StartListening();

        _commandSender = new TcpCommandSender();
    }

    private void OnStudentDiscovered(object? sender, StudentHelloPayload student)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var existing = _discoveredStudents.FirstOrDefault(s => s.Ip == student.Ip);
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
            }
        });
    }

    private async void LockBtn_Click(object sender, RoutedEventArgs e)
    {
        if (StudentsDataGrid.SelectedItem is StudentHelloPayload student)
        {
            await _commandSender.SendCommandAsync(student.Ip, "LOCK");
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
            await _commandSender.SendCommandAsync(student.Ip, "UNLOCK");
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

            await _commandSender.SendCommandAsync(student.Ip, "START_EXAM");
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
                await _commandSender.SendCommandAsync(student.Ip, "STOP_EXAM");
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
        if (_discoveredStudents.Count == 0)
        {
            MessageBox.Show("There are no students to generate a report for.", "Empty Report", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var generator = new PdfReportGenerator();
            string filePath = generator.GenerateReport(_discoveredStudents);

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

    protected override void OnClosed(EventArgs e)
    {
        _listener.Stop();
        _updateListener.Stop();
        base.OnClosed(e);
    }
}