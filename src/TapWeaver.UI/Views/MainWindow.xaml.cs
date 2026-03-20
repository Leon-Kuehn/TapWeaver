using System.Windows;
using System.Windows.Interop;
using TapWeaver.UI.ViewModels;

namespace TapWeaver.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        source.AddHook(WndProc);

        if (DataContext is MainViewModel vm)
        {
            // Apply persisted always-on-top setting
            Topmost = vm.AlwaysOnTop;

            // Keep Topmost in sync when the user changes it in Settings
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(MainViewModel.AlwaysOnTop))
                    Topmost = vm.AlwaysOnTop;
            };

            // Register global hotkeys now that we have a valid HWND
            vm.InitializeHotkeys(new WindowInteropHelper(this).Handle);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.Dispose();
        base.OnClosed(e);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;
        if (msg == WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            if (DataContext is MainViewModel vm)
                vm.HandleHotkey(id);
            handled = true;
        }
        return IntPtr.Zero;
    }
}
