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

    protected override void OnClosed(EventArgs e)
    {
        _listener.Stop();
        _updateListener.Stop();
        base.OnClosed(e);
    }
}