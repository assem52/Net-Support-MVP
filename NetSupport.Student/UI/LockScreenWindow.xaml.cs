using System.Windows;
using System.Windows.Input;

namespace NetSupport.Student.UI;

/// <summary>
/// A full-screen window that stays on top to prevent the student from doing anything.
/// </summary>
public partial class LockScreenWindow : Window
{
    // SAFETY FLAG: Set this to false before deploying to actual student PCs!
    // When true, the lock screen will NOT maximize so you can test locally without locking yourself out.
    public static bool IsLocalDebugMode = false;

    public LockScreenWindow()
    {
        InitializeComponent();
        
        if (IsLocalDebugMode)
        {
            this.WindowState = WindowState.Normal;
            this.Topmost = false;
            this.Width = 800;
            this.Height = 600;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Title = "Screen Locked (LOCAL DEBUG MODE)";
        }
    }

    private void EmergencyUnlock_Click(object sender, RoutedEventArgs e)
    {
        // Allows the user to escape the lock screen manually
        this.Close();
    }

    // Prevent Alt-F4 from closing the lock screen
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.System && e.SystemKey == Key.F4)
        {
            e.Handled = true;
        }
        base.OnPreviewKeyDown(e);
    }
}
