using System.Diagnostics;
using System.IO;
using MouseTranslator.Core.Selection;

namespace MouseTranslator.Infrastructure.Ocr;

public sealed class TesseractCommandLineOcrEngine : ILocalOcrEngine
{
    private const string Source = "OCR.Tesseract";
    private readonly string? _executablePath;
    private readonly string? _tessdataDirectory;

    public TesseractCommandLineOcrEngine(
        string? executablePath = null,
        string? tessdataDirectory = null)
    {
        _executablePath = executablePath ?? ResolveExecutablePath();
        _tessdataDirectory = tessdataDirectory ?? ResolveTessdataDirectory(_executablePath);
    }

    public async Task<OcrResult> RecognizeAsync(OcrRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(_executablePath))
        {
            return OcrResult.Failed(
                "Tesseract executable was not found. Install Tesseract or set MOUSE_TRANSLATOR_TESSERACT_PATH.",
                Source,
                TextExtractionFailureReason.NotSupported);
        }

        if (string.IsNullOrWhiteSpace(_tessdataDirectory))
        {
            return OcrResult.Failed(
                "Tesseract tessdata directory was not found. Place eng.traineddata under assets/ocr/tessdata or set MOUSE_TRANSLATOR_TESSDATA_PATH.",
                Source,
                TextExtractionFailureReason.NotSupported);
        }

        var languageModelPath = Path.Combine(_tessdataDirectory, $"{request.Language}.traineddata");
        if (!File.Exists(languageModelPath))
        {
            return OcrResult.Failed(
                $"Tesseract language data was not found: {languageModelPath}.",
                Source,
                TextExtractionFailureReason.NotSupported);
        }

        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            "MouseTranslator",
            "ocr",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        var inputFilePath = Path.Combine(tempDirectory, $"capture.{GetSafeExtension(request.ImageFormat)}");

        try
        {
            await File.WriteAllBytesAsync(inputFilePath, request.ImageBytes, cancellationToken);

            var startInfo = new ProcessStartInfo
            {
                FileName = _executablePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            startInfo.ArgumentList.Add(inputFilePath);
            startInfo.ArgumentList.Add("stdout");
            startInfo.ArgumentList.Add("--oem");
            startInfo.ArgumentList.Add("1");
            startInfo.ArgumentList.Add("-l");
            startInfo.ArgumentList.Add(request.Language);
            startInfo.ArgumentList.Add("--tessdata-dir");
            startInfo.ArgumentList.Add(_tessdataDirectory);
            startInfo.ArgumentList.Add("--psm");
            startInfo.ArgumentList.Add("6");
            startInfo.ArgumentList.Add("-c");
            startInfo.ArgumentList.Add("preserve_interword_spaces=1");
            startInfo.ArgumentList.Add("-c");
            startInfo.ArgumentList.Add("user_defined_dpi=300");

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return OcrResult.Failed(
                    "Failed to start Tesseract process.",
                    Source);
            }

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync(cancellationToken);

            var output = (await outputTask).Trim();
            var error = (await errorTask).Trim();

            if (process.ExitCode != 0)
            {
                return OcrResult.Failed(
                    string.IsNullOrWhiteSpace(error)
                        ? $"Tesseract exited with code {process.ExitCode}."
                        : error,
                    Source);
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                return OcrResult.Failed(
                    "Tesseract returned no text.",
                    Source,
                    TextExtractionFailureReason.EmptySelection);
            }

            return OcrResult.Succeeded(output, Source);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return OcrResult.Failed(
                $"Tesseract OCR failed: {ex.Message}",
                Source);
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    private static string? ResolveExecutablePath()
    {
        var candidates = new[]
        {
            Environment.GetEnvironmentVariable("MOUSE_TRANSLATOR_TESSERACT_PATH"),
            Path.Combine(AppContext.BaseDirectory, "assets", "ocr", "tesseract", "tesseract.exe"),
            Path.Combine(AppContext.BaseDirectory, "tesseract.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Tesseract-OCR", "tesseract.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Tesseract-OCR", "tesseract.exe"),
            ResolveFromPath("tesseract.exe"),
        };

        return candidates.FirstOrDefault(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path));
    }

    private static string? ResolveTessdataDirectory(string? executablePath)
    {
        var candidates = new[]
        {
            Environment.GetEnvironmentVariable("MOUSE_TRANSLATOR_TESSDATA_PATH"),
            Path.Combine(AppContext.BaseDirectory, "assets", "ocr", "tessdata"),
            executablePath is null ? null : Path.Combine(Path.GetDirectoryName(executablePath)!, "tessdata"),
        };

        return candidates.FirstOrDefault(path => !string.IsNullOrWhiteSpace(path) && Directory.Exists(path));
    }

    private static string? ResolveFromPath(string fileName)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        foreach (var segment in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(segment.Trim(), fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static string GetSafeExtension(string imageFormat)
    {
        return string.IsNullOrWhiteSpace(imageFormat)
            ? "png"
            : imageFormat.Trim().TrimStart('.').ToLowerInvariant();
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch
        {
        }
    }
}
