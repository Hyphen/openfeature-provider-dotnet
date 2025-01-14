using System.Text;
using System.Text.Json;

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
            _cache = new CacheClient(options.Cache);
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

        public async Task<EvaluationResponse> Evaluate(HyphenEvaluationContext context)
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

            return evaluationResponse;
        }

        public async Task PostTelemetry(TelemetryPayload payload)
        {
            await TryUrls("/toggle/telemetry", payload);
        }

        private async Task<string> TryUrls(string urlPath, object payload)
        {
            Exception lastException = null;

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

            throw lastException ?? new Exception("Failed to connect to any horizon URL");
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
        public HyphenEvaluationContext Context { get; set; }
        public TelemetryData Data { get; set; }
    }

    public class TelemetryData
    {
        public Evaluation Toggle { get; set; }
    }

    public class EvaluationResponse
    {
        public Dictionary<string, Evaluation> Toggles { get; set; }
    }

    public class Evaluation
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
        public string Reason { get; set; }
        public string ErrorMessage { get; set; }
    }
}
