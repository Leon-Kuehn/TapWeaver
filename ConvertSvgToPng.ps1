param(
    [string]$OutputDir = "c:\DEV\TapWeaver\src\TapWeaver.UI\Resources\Icons"
)

Add-Type -AssemblyName System.Drawing

function Write-IcoFromPngs {
    param(
        [string[]]$PngPaths,
        [string]$IcoPath
    )

    $stream = New-Object System.IO.MemoryStream
    $writer = New-Object System.IO.BinaryWriter($stream)

    $writer.Write([UInt16]0)
    $writer.Write([UInt16]1)
    $writer.Write([UInt16]$PngPaths.Count)

    $images = @()
    foreach ($path in $PngPaths) {
        $bytes = [System.IO.File]::ReadAllBytes($path)
        $size = [int]([System.IO.Path]::GetFileNameWithoutExtension($path))
        $images += [PSCustomObject]@{ Size = $size; Bytes = $bytes }
    }

    $offset = 6 + (16 * $images.Count)
    foreach ($image in $images) {
        $sizeByte = if ($image.Size -ge 256) { [byte]0 } else { [byte]$image.Size }
        $writer.Write($sizeByte)
        $writer.Write($sizeByte)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([UInt16]1)
        $writer.Write([UInt16]32)
        $writer.Write([UInt32]$image.Bytes.Length)
        $writer.Write([UInt32]$offset)
        $offset += $image.Bytes.Length
    }

    foreach ($image in $images) {
        $writer.Write($image.Bytes)
    }

    [System.IO.File]::WriteAllBytes($IcoPath, $stream.ToArray())
    $writer.Dispose()
    $stream.Dispose()
}

function New-LogoBitmap {
    param([int]$IconSize)

    $bitmap = New-Object System.Drawing.Bitmap -ArgumentList $IconSize, $IconSize
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([System.Drawing.Color]::Transparent)

    $startPoint = New-Object System.Drawing.Point -ArgumentList 0, 0
    $endPoint = New-Object System.Drawing.Point -ArgumentList $IconSize, $IconSize
    $backgroundBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush -ArgumentList $startPoint, $endPoint, ([System.Drawing.Color]::FromArgb(255, 30, 30, 46)), ([System.Drawing.Color]::FromArgb(255, 42, 42, 62))

    $graphics.FillRectangle($backgroundBrush, 0, 0, $IconSize, $IconSize)

    $strokeScale = [Math]::Max(1, [int]($IconSize / 24))
    $accentPen = New-Object System.Drawing.Pen -ArgumentList ([System.Drawing.Color]::FromArgb(255, 127, 219, 255)), (3 * $strokeScale)
    $accentPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $accentPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $accentPen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round

    $monitorPen = New-Object System.Drawing.Pen -ArgumentList ([System.Drawing.Color]::FromArgb(255, 58, 58, 92)), (2 * $strokeScale)
    $monitorRect = New-Object System.Drawing.Rectangle -ArgumentList ([int]($IconSize * 0.2)), ([int]($IconSize * 0.22)), ([int]($IconSize * 0.6)), ([int]($IconSize * 0.56))
    $graphics.DrawRectangle($monitorPen, $monitorRect)

    $barBrush = New-Object System.Drawing.SolidBrush -ArgumentList ([System.Drawing.Color]::FromArgb(255, 58, 58, 92))
    $graphics.FillRectangle($barBrush, [int]($IconSize * 0.27), [int]($IconSize * 0.29), [int]($IconSize * 0.46), [int]($IconSize * 0.06))

    $graphics.DrawBezier(
        $accentPen,
        [int]($IconSize * 0.28), [int]($IconSize * 0.4),
        [int]($IconSize * 0.38), [int]($IconSize * 0.33),
        [int]($IconSize * 0.46), [int]($IconSize * 0.64),
        [int]($IconSize * 0.5), [int]($IconSize * 0.64)
    )
    $graphics.DrawBezier(
        $accentPen,
        [int]($IconSize * 0.5), [int]($IconSize * 0.64),
        [int]($IconSize * 0.54), [int]($IconSize * 0.64),
        [int]($IconSize * 0.62), [int]($IconSize * 0.33),
        [int]($IconSize * 0.72), [int]($IconSize * 0.4)
    )

    $dotBrush = New-Object System.Drawing.SolidBrush -ArgumentList ([System.Drawing.Color]::FromArgb(255, 79, 195, 247))
    $dotSize = [Math]::Max(2, [int]($IconSize * 0.08))
    $dotX = [int]($IconSize * 0.5) - [int]($dotSize / 2)
    $dotY = [int]($IconSize * 0.64) - [int]($dotSize / 2)
    $graphics.FillEllipse($dotBrush, $dotX, $dotY, $dotSize, $dotSize)

    $backgroundBrush.Dispose()
    $accentPen.Dispose()
    $monitorPen.Dispose()
    $barBrush.Dispose()
    $dotBrush.Dispose()
    $graphics.Dispose()

    return $bitmap
}

if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

$sizes = @(16, 24, 32, 48, 64, 128, 256)
$pngPaths = @()

foreach ($iconSize in $sizes) {
    $bitmap = New-LogoBitmap -IconSize $iconSize
    $pngPath = Join-Path $OutputDir ("{0}.png" -f $iconSize)
    $bitmap.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
    $pngPaths += $pngPath
    Write-Host ("PNG generated: {0}" -f $pngPath)
}

$icoPath = Join-Path $OutputDir "tapweaver.ico"
Write-IcoFromPngs -PngPaths $pngPaths -IcoPath $icoPath
Write-Host ("ICO generated: {0}" -f $icoPath)
