using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using TapWeaver.UI.ViewModels;

namespace TapWeaver.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
       LoadAppIcon();
    }

    private void LoadAppIcon()
    {
        try
        {
            // Load the PNG icon from resources
            var iconUri = new Uri("pack://application:,,,/Assets/AppIcon.png");
            this.Icon = new BitmapImage(iconUri);
        }
        catch
        {
            // Gracefully fall back to default icon if loading fails
        }
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
            ApplyCompactMode(vm.CompactMode);

            // Keep Topmost in sync when the user changes it in Settings
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(MainViewModel.AlwaysOnTop))
                    Topmost = vm.AlwaysOnTop;
                else if (args.PropertyName == nameof(MainViewModel.CompactMode))
                    ApplyCompactMode(vm.CompactMode);
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

    private void ApplyCompactMode(bool compactMode)
    {
        if (compactMode)
        {
            MinWidth = 720;
            MinHeight = 480;
            if (Width > 920)
                Width = 920;
            if (Height > 620)
                Height = 620;
            return;
        }

        MinWidth = 920;
        MinHeight = 620;
        if (Width < 1000)
            Width = 1180;
        if (Height < 680)
            Height = 760;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximizeRestore();
            return;
        }

        DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleMaximizeRestore();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ToggleMaximizeRestore()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }
}
