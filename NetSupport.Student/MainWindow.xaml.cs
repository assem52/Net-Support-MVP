using System.Windows;
using System.Windows.Controls;

namespace NetSupport.Student;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MachineNameText.Text = Environment.MachineName;
    }

    private void RaiseHandBtn_Click(object sender, RoutedEventArgs e)
    {
        var app = (App)Application.Current;
        var msg = new NetSupport.Shared.Models.NetworkMessage
        {
            Type = "RAISE_HAND",
            Payload = System.Text.Json.JsonSerializer.SerializeToElement(new NetSupport.Shared.Models.RaiseHandPayload
            {
                Ip = string.Empty,
                IsRaising = true,
                Message = "Student needs help."
            })
        };
        app.SendUpdate(msg);
        
        RaiseHandBtn.IsEnabled = false;
        RaiseHandBtn.Opacity = 0.5;
        if (RaiseHandBtn.Content is StackPanel sp && sp.Children.Count > 1 && sp.Children[1] is TextBlock tb)
        {
            tb.Text = "Help Requested...";
        }
    }
}