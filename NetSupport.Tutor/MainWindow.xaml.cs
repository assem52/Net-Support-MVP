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
    
    // ObservableCollection automatically updates the UI when items are added
    private ObservableCollection<StudentHelloPayload> _discoveredStudents;

    public MainWindow()
    {
        InitializeComponent();
        
        _discoveredStudents = new ObservableCollection<StudentHelloPayload>();
        
        // Bind the DataGrid to our collection
        StudentsDataGrid.ItemsSource = _discoveredStudents;

        // Initialize and start listening for students
        _listener = new UdpListener();
        _listener.StudentDiscovered += OnStudentDiscovered;
        _listener.StartListening();
    }

    private void OnStudentDiscovered(object? sender, StudentHelloPayload student)
    {
        // UI updates must happen on the main UI thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Check if we already have this student in the list to avoid duplicates
            var existing = _discoveredStudents.FirstOrDefault(s => s.Ip == student.Ip);
            if (existing == null)
            {
                _discoveredStudents.Add(student);
            }
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        // Always clean up network resources when closing
        _listener.Stop();
        base.OnClosed(e);
    }
}
