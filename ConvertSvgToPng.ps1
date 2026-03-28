[void][System.Reflection.Assembly]::LoadWithPartialName("PresentationCore")
[void][System.Reflection.Assembly]::LoadWithPartialName("PresentationFramework")
[void][System.Reflection.Assembly]::LoadWithPartialName("WindowsBase")

# SVG to PNG conversion using WPF
$svgPath = "c:\DEV\TapWeaver\src\TapWeaver.UI\Assets\AppIcon.svg"
$pngPath = "c:\DEV\TapWeaver\src\TapWeaver.UI\Assets\AppIcon.png"
$icoPath = "c:\DEV\TapWeaver\src\TapWeaver.UI\Assets\AppIcon.ico"

# Create a DrawingImage from SVG and render to PNG
try {
    # Read SVG as string
    $svgContent = Get-Content $svgPath -Raw
    
    # Use a simple approach: render SVG with a RenderTargetBitmap
    $renderTarget = New-Object System.Windows.Media.Imaging.RenderTargetBitmap(256, 256, 96, 96, [System.Windows.Media.PixelFormats]::Pbgra32)
    
    # Create a DrawingVisual with the SVG rendered
    $visual = New-Object System.Windows.Media.DrawingVisual
    $dc = $visual.RenderOpen()
    
    # Parse SVG and get a basic rectangle placeholder (SVG parsing is complex)
    # For now, we'll create a colored square as a placeholder 
    $brush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Colors]::CornflowerBlue)
    $pen = New-Object System.Windows.Media.Pen($brush, 2)
    
    $dc.DrawRectangle($brush, $pen, (New-Object System.Windows.Rect(10, 10, 236, 236)))
    
    # Draw "TW" text
    $typeface = New-Object System.Windows.Media.Typeface("Arial", [System.Windows.FontStyles]::Normal, [System.Windows.FontWeights]::Bold, [System.Windows.FontStretches]::Normal)
    $formattedText = New-Object System.Windows.Media.FormattedText("TW", [System.Globalization.CultureInfo]::InvariantCulture, [System.Windows.FlowDirection]::LeftToRight, $typeface, 80, [System.Windows.Media.Brushes]::White)
    
    $textPoint = New-Object System.Windows.Point(80, 85)
    $dc.DrawText($formattedText, $textPoint)
    $dc.Close()
    
    # Render to bitmap
    $renderTarget.Render($visual)
    
    # Save as PNG
    $encoder = New-Object System.Windows.Media.Imaging.PngBitmapEncoder
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($renderTarget))
    
    $fileStream = New-Object System.IO.FileStream($pngPath, [System.IO.FileMode]::Create)
    $encoder.Save($fileStream)
    $fileStream.Close()
    
    Write-Host "✓ PNG created: $pngPath"
} catch {
    Write-Host "✗ Error creating PNG: $_"
}
