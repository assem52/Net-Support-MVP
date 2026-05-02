using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace NetSupport.Student.UI
{
    public partial class LockScreenWindow : Window
    {
        public static bool IsLocalDebugMode = false;

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static IntPtr _hookID = IntPtr.Zero;
        private LowLevelKeyboardProc _proc;

        public LockScreenWindow()
        {
            InitializeComponent();
            _proc = HookCallback;

            if (!IsLocalDebugMode)
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;
                this.Topmost = true;
                this.ShowInTaskbar = false;

                _hookID = SetHook(_proc);
            }
            else
            {
                this.Title = "DEBUG MODE - Keyboard Unlocked";
            }
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(13, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                if (key == Key.LWin || key == Key.RWin ||
                    key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt ||
                    key == Key.Escape && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control ||
                    key == Key.Escape && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                {
                    return (IntPtr)1; 
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            base.OnClosing(e);
        }

        private void EmergencyUnlock_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.System && e.SystemKey == Key.F4)
            {
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }
    }
}