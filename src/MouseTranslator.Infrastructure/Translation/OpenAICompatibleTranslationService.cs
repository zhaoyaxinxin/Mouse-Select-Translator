using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MouseTranslator.Core.Translation;

namespace MouseTranslator.Infrastructure.Translation;

public sealed class OpenAICompatibleTranslationService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _model;
    private readonly string _apiKeyEnvironmentVariable;

    public OpenAICompatibleTranslationService(
        HttpClient httpClient,
        string baseUrl,
        string model,
        string apiKeyEnvironmentVariable)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
        _model = model;
        _apiKeyEnvironmentVariable = apiKeyEnvironmentVariable;
    }

    public async Task<TranslationResult> TranslateAsync(
        TranslationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_baseUrl) || string.IsNullOrWhiteSpace(_model))
        {
            return TranslationResult.Failed("OpenAI-compatible translation is not configured.");
        }

        var apiKey = Environment.GetEnvironmentVariable(_apiKeyEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return TranslationResult.Failed(
                $"Missing API key environment variable: {_apiKeyEnvironmentVariable}");
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(new
            {
                model = _model,
                temperature = 0.2,
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content =
                            $"You are a translation assistant. Translate the user's selected text into {request.TargetLanguage}. " +
                            "Return only the translated result. Do not repeat the source text. Do not explain. Do not add quotation marks.",
                    },
                    new
                    {
                        role = "user",
                        content = request.Text,
                    },
                },
            }),
            Encoding.UTF8,
            "application/json");

        try
        {
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return TranslationResult.Failed(
                    $"Translation failed with HTTP {(int)response.StatusCode}.");
            }

            using var document = JsonDocument.Parse(responseText);
            var content = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
            {
                return TranslationResult.Failed("Translation response was empty.");
            }

            return TranslationResult.Succeeded(content.Trim(), request.SourceLanguage, request.TargetLanguage);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return TranslationResult.Failed(ex.Message);
        }
    }
}
