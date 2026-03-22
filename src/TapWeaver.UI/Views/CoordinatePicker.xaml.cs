using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;

namespace TapWeaver.UI.Views;

public partial class CoordinatePicker : Window
{
    public int? SelectedX { get; private set; }
    public int? SelectedY { get; private set; }

    public CoordinatePicker()
    {
        InitializeComponent();
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(RootGrid);
        
        CrosshairCanvas.Children.Clear();
        DrawCrosshair((int)pos.X, (int)pos.Y);
    }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(RootGrid);
        SelectedX = (int)pos.X;
        SelectedY = (int)pos.Y;
        DialogResult = true;
        Close();
    }

    private void DrawCrosshair(int x, int y)
    {
        var lineColor = new SolidColorBrush(Color.FromArgb(200, 255, 0, 0));
        
        // Vertical line
        var vLine = new Line
        {
            X1 = x,
            Y1 = 0,
            X2 = x,
            Y2 = RootGrid.ActualHeight,
            Stroke = lineColor,
            StrokeThickness = 1,
            Opacity = 0.7
        };
        CrosshairCanvas.Children.Add(vLine);

        // Horizontal line
        var hLine = new Line
        {
            X1 = 0,
            Y1 = y,
            X2 = RootGrid.ActualWidth,
            Y2 = y,
            Stroke = lineColor,
            StrokeThickness = 1,
            Opacity = 0.7
        };
        CrosshairCanvas.Children.Add(hLine);

        // Center circle
        var circle = new Ellipse
        {
            Width = 10,
            Height = 10,
            Stroke = lineColor,
            StrokeThickness = 1,
            Opacity = 0.8
        };
        Canvas.SetLeft(circle, x - 5);
        Canvas.SetTop(circle, y - 5);
        CrosshairCanvas.Children.Add(circle);
    }
}
