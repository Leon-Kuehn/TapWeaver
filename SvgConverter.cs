using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

class SvgToPngConverter
{
    static void Main()
    {
        string svgPath = @"c:\DEV\TapWeaver\src\TapWeaver.UI\Assets\AppIcon.svg";
        string pngPath = @"c:\DEV\TapWeaver\src\TapWeaver.UI\Assets\AppIcon.png";

        try
        {
            // Create a DrawingVisual to render the icon
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                // Background
                var bgBrush = new RadialGradientBrush(Color.FromRgb(30, 30, 40), Color.FromRgb(20, 20, 30));
                dc.DrawRectangle(bgBrush, null, new Rect(0, 0, 256, 256));

                // Outer cyan halo circle
                var haloBrush = new SolidColorBrush(Color.FromArgb(100, 0, 200, 255));
                var haloPen = new Pen(haloBrush, 3);
                dc.DrawEllipse(null, haloPen, new Point(128, 128), 110, 110);

                // Main circle background
                var circleBrush = new SolidColorBrush(Color.FromRgb(40, 50, 80));
                dc.DrawEllipse(circleBrush, null, new Point(128, 128), 100, 100);

                // "TW" text
                var typeface = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
                var formattedText = new FormattedText("TW", 
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    100,
                    Brushes.White);

                dc.DrawText(formattedText, new Point(100, 85));

                // Waveform line (automation indicator)
                var wavePen = new Pen(new SolidColorBrush(Color.FromRgb(0, 200, 255)), 2);
                double[] wavePoints = { 190, 160, 200, 150, 210, 160, 220, 140, 230, 155 };
                for (int i = 0; i < wavePoints.Length - 2; i += 2)
                {
                    dc.DrawLine(wavePen, new Point(wavePoints[i], wavePoints[i + 1]), new Point(wavePoints[i + 2], wavePoints[i + 3]));
                }
            }

            // Render to bitmap
            var renderTarget = new RenderTargetBitmap(256, 256, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(visual);

            // Save as PNG
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTarget));

            using (var file = System.IO.File.Create(pngPath))
            {
                encoder.Save(file);
            }

            Console.WriteLine($"✓ PNG created successfully: {pngPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }
}
