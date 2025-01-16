using System.Text;
using System.Text.Json;
using OpenFeature.Model;

namespace Hyphen.OpenFeature.Provider
{
    public class HyphenClient
    {
        private readonly string _publicKey;
        private readonly string[] _horizonUrls;
        private readonly string _defaultHorizonUrl;
        private readonly CacheClient _cache;
        private readonly HttpClient _httpClient;

        public HyphenClient(string publicKey, HyphenProviderOptions options)
        {
            _publicKey = publicKey;
            _defaultHorizonUrl = BuildDefaultHorizonUrl(publicKey);
            var urls = options.HorizonUrls ?? Array.Empty<string>();
            _horizonUrls = new List<string>(urls) { _defaultHorizonUrl }.ToArray();
            var cache = options.Cache ?? new CacheOptions();
            _cache = new CacheClient(cache);
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", publicKey);
        }

        private string BuildDefaultHorizonUrl(string publicKey)
        {
            var orgId = GetOrgIdFromPublicKey(publicKey);
            return !string.IsNullOrEmpty(orgId) 
                ? $"https://{orgId}.toggle.hyphen.cloud" 
                : "https://toggle.hyphen.cloud";
        }

        static string? GetOrgIdFromPublicKey(string publicKey)
        {
            try
            {
                var keyWithoutPrefix = publicKey.Replace("public_", "");
                var decoded = Convert.FromBase64String(keyWithoutPrefix);
                var decodedString = Encoding.UTF8.GetString(decoded);
                var orgId = decodedString.Split(':')[0];
                return System.Text.RegularExpressions.Regex.IsMatch(orgId, "^[a-zA-Z0-9_-]+$") ? orgId : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<EvaluationResponse> Evaluate(EvaluationContext context)
        {
            var cachedResponse = _cache.Get<EvaluationResponse>(context);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var response = await TryUrls("/toggle/evaluate", context);
            var evaluationResponse = JsonSerializer.Deserialize<EvaluationResponse>(response);

            if (evaluationResponse != null)
            {
                _cache.Set(context, evaluationResponse);
            }

            return evaluationResponse!;
        }

        public async Task PostTelemetry(TelemetryPayload payload)
        {
            await TryUrls("/toggle/telemetry", payload);
        }

        private async Task<string> TryUrls(string urlPath, object payload)
        {
            Exception lastException = new Exception("Failed to connect to any horizon URL");

            foreach (var baseUrl in _horizonUrls)
            {
                try
                {
                    var url = new Uri(new Uri(baseUrl), urlPath.TrimStart('/')).ToString();
                    var response = await HttpPost(url, payload);
                    return response;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            throw lastException;
        }

        private async Task<string> HttpPost(string url, object payload)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed: {responseContent}");
            }

            return responseContent;
        }
    }

    public class TelemetryPayload
    {
        public required EvaluationContext Context { get; set; }
        public required TelemetryData Data { get; set; }
    }

    public class TelemetryData
    {
        public required Evaluation Toggle { get; set; }
    }

    public class EvaluationResponse
    {
        public required Dictionary<string, Evaluation> toggles { get; set; }
    }

    public class Evaluation
    {
        public required string key { get; set; }
        public required object value { get; set; }
        public required string type { get; set; }
        public string? reason { get; set; }
        public string? errorMessage { get; set; }
    }
}
