using System.Net;
using System.Text;
using System.Text.Json;

namespace MouseTranslator.Infrastructure.TextExtraction;

public sealed class BrowserSelectionLoopbackServer : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly BrowserSelectionBuffer _buffer;
    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private Task? _backgroundTask;
    private bool _disposed;

    public BrowserSelectionLoopbackServer(
        BrowserSelectionBuffer buffer,
        string listenerPrefix = "http://127.0.0.1:48331/")
    {
        _buffer = buffer;
        ListenerPrefix = listenerPrefix;
        _listener.Prefixes.Add(listenerPrefix);
    }

    public string ListenerPrefix { get; }

    public string SelectionEndpoint => $"{ListenerPrefix.TrimEnd('/')}/selection";

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_backgroundTask is not null)
        {
            return;
        }

        _listener.Start();
        _backgroundTask = Task.Run(() => RunAsync(_cancellationTokenSource.Token));
    }

    public void Stop()
    {
        if (_backgroundTask is null)
        {
            return;
        }

        _cancellationTokenSource.Cancel();

        if (_listener.IsListening)
        {
            _listener.Stop();
        }

        try
        {
            _backgroundTask.Wait(TimeSpan.FromSeconds(2));
        }
        catch
        {
        }

        _backgroundTask = null;
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            HttpListenerContext? context = null;

            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (HttpListenerException) when (cancellationToken.IsCancellationRequested || !_listener.IsListening)
            {
                break;
            }
            catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (context is not null)
            {
                _ = Task.Run(() => HandleContextAsync(context, cancellationToken), cancellationToken);
            }
        }
    }

    private async Task HandleContextAsync(HttpListenerContext context, CancellationToken cancellationToken)
    {
        try
        {
            var path = context.Request.Url?.AbsolutePath?.TrimEnd('/') ?? string.Empty;
            if (context.Request.HttpMethod == "GET" && string.Equals(path, "/health", StringComparison.OrdinalIgnoreCase))
            {
                await WriteResponseAsync(context.Response, 200, "ok", cancellationToken);
                return;
            }

            if (context.Request.HttpMethod != "POST"
                || !string.Equals(path, "/selection", StringComparison.OrdinalIgnoreCase))
            {
                await WriteResponseAsync(context.Response, 404, "not found", cancellationToken);
                return;
            }

            var payload = await JsonSerializer.DeserializeAsync<BrowserSelectionPayload>(
                context.Request.InputStream,
                JsonOptions,
                cancellationToken);

            if (payload is null || string.IsNullOrWhiteSpace(payload.Text))
            {
                await WriteResponseAsync(context.Response, 400, "invalid payload", cancellationToken);
                return;
            }

            var normalizedPayload = payload with
            {
                Text = payload.Text.Trim(),
                CapturedAtUtc = payload.CapturedAtUtc == default ? DateTimeOffset.UtcNow : payload.CapturedAtUtc,
            };

            _buffer.Store(normalizedPayload);
            await WriteResponseAsync(context.Response, 202, "accepted", cancellationToken);
        }
        catch (Exception)
        {
            if (!context.Response.OutputStream.CanWrite)
            {
                return;
            }

            await WriteResponseAsync(context.Response, 500, "error", cancellationToken);
        }
        finally
        {
            context.Response.Close();
        }
    }

    private static async Task WriteResponseAsync(
        HttpListenerResponse response,
        int statusCode,
        string body,
        CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        response.StatusCode = statusCode;
        response.ContentType = "text/plain; charset=utf-8";
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes, cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Stop();
        _listener.Close();
        _cancellationTokenSource.Dispose();
    }
}
