using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using MouseTranslator.Core.Selection;

namespace MouseTranslator.Infrastructure.Ocr;

public sealed class GdiScreenCaptureService : IScreenCaptureService
{
    private const int OutputScale = 2;
    private const float OutputDpi = 300f;

    public Task<ScreenCaptureResult> CaptureAsync(
        ScreenCaptureRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.Region.Width <= 0 || request.Region.Height <= 0)
        {
            return Task.FromResult(ScreenCaptureResult.Failed(
                "Screen capture region must be greater than zero.",
                request.ImageFormat));
        }

        if (!string.Equals(request.ImageFormat, "png", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(ScreenCaptureResult.Failed(
                $"Unsupported image format: {request.ImageFormat}.",
                request.ImageFormat));
        }

        try
        {
            using var bitmap = new Bitmap(request.Region.Width, request.Region.Height, PixelFormat.Format32bppArgb);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(
                request.Region.X,
                request.Region.Y,
                0,
                0,
                new Size(request.Region.Width, request.Region.Height),
                CopyPixelOperation.SourceCopy);

            using var processedBitmap = Preprocess(bitmap);
            using var stream = new MemoryStream();
            processedBitmap.Save(stream, ImageFormat.Png);
            return Task.FromResult(ScreenCaptureResult.Succeeded(stream.ToArray(), "png"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ScreenCaptureResult.Failed(
                $"Screen capture failed: {ex.Message}",
                request.ImageFormat));
        }
    }

    private static Bitmap Preprocess(Bitmap source)
    {
        var scaledWidth = Math.Max(1, source.Width * OutputScale);
        var scaledHeight = Math.Max(1, source.Height * OutputScale);
        var scaled = new Bitmap(scaledWidth, scaledHeight, PixelFormat.Format24bppRgb);
        scaled.SetResolution(OutputDpi, OutputDpi);

        using (var graphics = Graphics.FromImage(scaled))
        {
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.Clear(Color.White);
            graphics.DrawImage(source, new Rectangle(0, 0, scaledWidth, scaledHeight));
        }

        ApplyGrayscaleContrast(scaled);
        return scaled;
    }

    private static void ApplyGrayscaleContrast(Bitmap bitmap)
    {
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                var grayscale = (int)Math.Round((pixel.R * 0.299) + (pixel.G * 0.587) + (pixel.B * 0.114));
                var contrasted = Math.Clamp((int)Math.Round(((grayscale - 128) * 1.35) + 128), 0, 255);
                bitmap.SetPixel(x, y, Color.FromArgb(contrasted, contrasted, contrasted));
            }
        }
    }
}
