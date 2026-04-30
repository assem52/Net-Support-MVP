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

    private async void LockBtn_Click(object sender, RoutedEventArgs e)
    {
        if (StudentsDataGrid.SelectedItem is StudentHelloPayload student)
        {
            await _commandSender.SendCommandAsync(student.Ip, "LOCK");
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
        }
        else
        {
            MessageBox.Show("Please select a student from the list first.", "No Student Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _listener.Stop();
        base.OnClosed(e);
    }
}
