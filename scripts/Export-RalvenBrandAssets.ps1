[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$brandRoot = Join-Path $repositoryRoot 'assets\brand'
$receivedRoot = Join-Path $brandRoot 'source\received'
$sourceIcon = Join-Path $receivedRoot 'ralven-app-icon-original.png'
$sourceBackground = Join-Path $receivedRoot 'ralven-atmosphere-background-original.png'
$iconExportRoot = Join-Path $brandRoot 'export\app-icon'
$backgroundExportRoot = Join-Path $brandRoot 'export\background'
$appAssetRoot = Join-Path $repositoryRoot 'src\Ralven.App\Assets'
$websitePublicRoot = Join-Path $repositoryRoot 'website\public'
$websiteFontRoot = Join-Path $websitePublicRoot 'fonts'
$docsAssetRoot = Join-Path $repositoryRoot 'docs\assets'
$dashboardAssetRoot = Join-Path $repositoryRoot 'infra\dashboard\assets\img'

$immutableSourceHashes = [ordered]@{
    'ralven-app-icon-original.png' = '07B4C6E60C1AD68CB57162BF7F10D81BABCF060F47BD0022C182658A9773C928'
    'ralven-atmosphere-background-original.png' = '9ABC3C4923DDD051D1CBA62EE4A1DD0C73BCF36AF79073531888F4D357A60A1C'
    'guidelines\ralven-brand-guidelines-01.png' = 'FD8133667CD6A24C211E9FCF8589D1D12F20AE175A247AFD30C5EF0A21F5274E'
    'guidelines\ralven-brand-guidelines-02.png' = 'D004FC8226DD1EAAA67198CC213E47F7970DF7714C8B1649F26C019A7A6BD543'
    'guidelines\ralven-brand-guidelines-03.png' = 'CA5AA4378165FCBB01E0F4D43A3867EE204D150A203FDC6E2B2737DCBFCECAED'
    'guidelines\ralven-brand-guidelines-04.png' = '3ACDB234B0CB009DDC9C5A010CBE947A6A75B26109B0D46E22DF073BC97BAEBB'
    'guidelines\ralven-brand-guidelines-05.png' = '4DF3AC9CC2AA21CCF5F7CF9D072DFBB827F06CCE6F7AD6D22534E47223AC9AAD'
    'guidelines\ralven-brand-guidelines-06.png' = 'D187D78040CD5EFE707D3B3C598221A273532FECBF56D39BCBC2AC048F4A9771'
}

foreach ($entry in $immutableSourceHashes.GetEnumerator()) {
    $path = Join-Path $receivedRoot $entry.Key
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required immutable brand source is missing: $path"
    }

    $actualHash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
    if ($actualHash -ne $entry.Value) {
        throw "Immutable brand source changed: $($entry.Key). Expected $($entry.Value), got $actualHash."
    }
}

New-Item -ItemType Directory -Path `
    $iconExportRoot, $backgroundExportRoot, $appAssetRoot, $websiteFontRoot, $docsAssetRoot, $dashboardAssetRoot `
    -Force | Out-Null

Add-Type -AssemblyName System.Drawing.Common
$drawingAssemblyRoot = Split-Path -Parent ([System.Drawing.Bitmap].Assembly.Location)
$drawingAssemblies = @(
    [System.Drawing.Bitmap].Assembly.Location
    [System.Drawing.Rectangle].Assembly.Location
    (Join-Path $drawingAssemblyRoot 'System.Private.CoreLib.dll')
    (Join-Path $drawingAssemblyRoot 'System.Private.Windows.GdiPlus.dll')
    (Join-Path $drawingAssemblyRoot 'System.Private.Windows.Core.dll')
    (Join-Path $drawingAssemblyRoot 'System.Runtime.dll')
    (Join-Path $drawingAssemblyRoot 'System.Collections.dll')
    (Join-Path $drawingAssemblyRoot 'System.IO.dll')
    (Join-Path $drawingAssemblyRoot 'System.Memory.dll')
    (Join-Path $drawingAssemblyRoot 'System.Runtime.InteropServices.dll')
)

if (-not ('RalvenBrandAssets.ImageExporter' -as [type])) {
    Add-Type -Language CSharp -ReferencedAssemblies $drawingAssemblies -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace RalvenBrandAssets
{
    public static class ImageExporter
    {
        public static void ExportSquarePngs(string sourcePath, string outputDirectory, int alphaThreshold, int[] sizes)
        {
            using (var source = new Bitmap(sourcePath))
            using (var argb = CopyToArgb(source))
            {
                var bounds = FindAlphaBounds(argb, alphaThreshold);
                if (bounds.Width == 0 || bounds.Height == 0)
                {
                    throw new InvalidDataException("The source icon contains no visible pixels.");
                }

                var side = Math.Max(bounds.Width, bounds.Height);
                using (var square = new Bitmap(side, side, PixelFormat.Format32bppArgb))
                {
                    using (var graphics = Graphics.FromImage(square))
                    {
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        graphics.Clear(Color.Transparent);
                        var x = (side - bounds.Width) / 2 - bounds.X;
                        var y = (side - bounds.Height) / 2 - bounds.Y;
                        graphics.DrawImageUnscaled(argb, x, y);
                    }

                    foreach (var size in sizes)
                    {
                        if (size <= 0)
                        {
                            throw new ArgumentOutOfRangeException(nameof(sizes), "Export sizes must be positive.");
                        }

                        using (var resized = Resize(square, size))
                        using (var stream = new MemoryStream())
                        {
                            resized.Save(stream, ImageFormat.Png);
                            File.WriteAllBytes(
                                Path.Combine(outputDirectory, $"ralven-app-icon-{size}.png"),
                                stream.ToArray());
                        }
                    }
                }
            }
        }

        public static void BuildPngBackedIco(string outputPath, string pngDirectory, int[] sizes)
        {
            var frames = new byte[sizes.Length][];
            for (var index = 0; index < sizes.Length; index++)
            {
                frames[index] = File.ReadAllBytes(Path.Combine(pngDirectory, $"ralven-app-icon-{sizes[index]}.png"));
            }

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((ushort)0);
                writer.Write((ushort)1);
                writer.Write((ushort)frames.Length);

                var imageOffset = 6 + (16 * frames.Length);
                for (var index = 0; index < frames.Length; index++)
                {
                    var size = sizes[index];
                    writer.Write(size >= 256 ? (byte)0 : (byte)size);
                    writer.Write(size >= 256 ? (byte)0 : (byte)size);
                    writer.Write((byte)0);
                    writer.Write((byte)0);
                    writer.Write((ushort)1);
                    writer.Write((ushort)32);
                    writer.Write((uint)frames[index].Length);
                    writer.Write((uint)imageOffset);
                    imageOffset += frames[index].Length;
                }

                foreach (var frame in frames)
                {
                    writer.Write(frame);
                }

                File.WriteAllBytes(outputPath, stream.ToArray());
            }
        }

        private static Bitmap CopyToArgb(Bitmap source)
        {
            var copy = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(copy))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.DrawImageUnscaled(source, 0, 0);
            }

            return copy;
        }

        private static Rectangle FindAlphaBounds(Bitmap bitmap, int alphaThreshold)
        {
            var rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var stride = Math.Abs(data.Stride);
                var pixels = new byte[stride * bitmap.Height];
                Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

                var minX = bitmap.Width;
                var minY = bitmap.Height;
                var maxX = -1;
                var maxY = -1;

                for (var y = 0; y < bitmap.Height; y++)
                {
                    var row = data.Stride >= 0 ? y * stride : (bitmap.Height - 1 - y) * stride;
                    for (var x = 0; x < bitmap.Width; x++)
                    {
                        if (pixels[row + (x * 4) + 3] < alphaThreshold)
                        {
                            continue;
                        }

                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                    }
                }

                return maxX < minX
                    ? Rectangle.Empty
                    : Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        private static Bitmap Resize(Bitmap source, int size)
        {
            var target = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(target))
            using (var attributes = new ImageAttributes())
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.Clear(Color.Transparent);
                attributes.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(
                    source,
                    new Rectangle(0, 0, size, size),
                    0,
                    0,
                    source.Width,
                    source.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }

            return target;
        }
    }
}
'@
}

$pngSizes = @(16, 20, 24, 32, 40, 48, 64, 96, 128, 192, 256, 512, 1024)
$icoSizes = @(16, 20, 24, 32, 40, 48, 64, 128, 256)
$exportIco = Join-Path $iconExportRoot 'Ralven.ico'

# Alpha 1 is isolated generation noise far outside the visible tile. Pixels
# with alpha >= 2 are kept byte-for-byte before centered square padding.
[RalvenBrandAssets.ImageExporter]::ExportSquarePngs($sourceIcon, $iconExportRoot, 2, $pngSizes)
[RalvenBrandAssets.ImageExporter]::BuildPngBackedIco($exportIco, $iconExportRoot, $icoSizes)

function Copy-IfChanged {
    param(
        [Parameter(Mandatory)] [string] $Source,
        [Parameter(Mandatory)] [string] $Destination
    )

    if ((Test-Path -LiteralPath $Destination -PathType Leaf) -and
        ((Get-FileHash -LiteralPath $Source -Algorithm SHA256).Hash -eq
            (Get-FileHash -LiteralPath $Destination -Algorithm SHA256).Hash)) {
        return
    }

    [System.IO.File]::Copy($Source, $Destination, $true)
}

$exportBackground = Join-Path $backgroundExportRoot 'ralven-atmosphere-1672x941.png'
Copy-IfChanged -Source $sourceBackground -Destination $exportBackground
Copy-IfChanged -Source (Join-Path $iconExportRoot 'ralven-app-icon-1024.png') -Destination (Join-Path $appAssetRoot 'Ralven.png')
Copy-IfChanged -Source $exportIco -Destination (Join-Path $appAssetRoot 'Ralven.ico')
foreach ($destination in @(
    (Join-Path $websitePublicRoot 'icon.png'),
    (Join-Path $docsAssetRoot 'icon.png'),
    (Join-Path $dashboardAssetRoot 'logo.png')
)) {
    Copy-IfChanged -Source (Join-Path $iconExportRoot 'ralven-app-icon-512.png') -Destination $destination
}
Copy-IfChanged -Source $exportBackground -Destination (Join-Path $websitePublicRoot 'og.png')
Copy-IfChanged -Source $exportBackground -Destination (Join-Path $docsAssetRoot 'hero-ralven.png')
Copy-IfChanged `
    -Source (Join-Path $brandRoot 'fonts\inter-4.1\web\InterVariable.woff2') `
    -Destination (Join-Path $websiteFontRoot 'InterVariable.woff2')
Copy-IfChanged `
    -Source (Join-Path $brandRoot 'fonts\inter-4.1\web\InterVariable-Italic.woff2') `
    -Destination (Join-Path $websiteFontRoot 'InterVariable-Italic.woff2')

$checksumPath = Join-Path $brandRoot 'CHECKSUMS.sha256'
$checksumLines = Get-ChildItem -LiteralPath $brandRoot -Recurse -File |
    Where-Object { $_.FullName -ne $checksumPath } |
    ForEach-Object {
        $relativePath = [System.IO.Path]::GetRelativePath($brandRoot, $_.FullName).Replace('\', '/')
        $hash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        "$hash  $relativePath"
    } |
    Sort-Object

$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText($checksumPath, (($checksumLines -join "`n") + "`n"), $utf8WithoutBom)

Write-Host "Ralven brand exports regenerated from verified immutable sources."
