using System.Windows;
using System.Windows.Input;

namespace NetSupport.Student.UI;

/// <summary>
/// A full-screen window that stays on top to prevent the student from doing anything.
/// </summary>
public partial class LockScreenWindow : Window
{
    public LockScreenWindow()
    {
        InitializeComponent();
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
