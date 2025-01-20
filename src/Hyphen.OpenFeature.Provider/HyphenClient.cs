using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OpenFeature.Model;

namespace Hyphen.OpenFeature.Provider
{
    public class HyphenClient
    {
        private readonly string _publicKey;
        private readonly HyphenProviderOptions _options;
        private readonly string[] _horizonUrls;
        private readonly string _defaultHorizonUrl;
        private readonly CacheClient _cache;
        private readonly HttpClient _httpClient;

        public HyphenClient(string publicKey, HyphenProviderOptions options)
        {
            _publicKey = publicKey;
            _options = options;
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
            HyphenEvaluationContext payload = BuildPayloadFromContext(context);
            var response = await TryUrls("/toggle/evaluate", payload);
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
                    string? url = new Uri(new Uri(baseUrl), urlPath.TrimStart('/')).ToString();
                    string? response = await HttpPost(url, payload);
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
            StringContent content = new StringContent(
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

        public HyphenEvaluationContext BuildPayloadFromContext(EvaluationContext context)
        {
            string? application = context.ContainsKey("Application") ?  context.GetValue("Application").AsString : _options.Application;
            string? environment = context.ContainsKey("Environment") ? context.GetValue("Environment").AsString : _options.Environment;
            string? ipAddress = context.ContainsKey("IpAddress") ? context.GetValue("IpAddress").AsString : null;
            Structure? userContext = context.ContainsKey("User") ? context.GetValue("User").AsStructure : null;
            Dictionary<string, object> customAttributes = new Dictionary<string, object>();

            if (context.TryGetValue("CustomAttributes", out var customAttributesValue) &&
            customAttributesValue?.AsStructure is Structure customAttributesStructure)
            {
                foreach (var key in customAttributesStructure.Keys)
                {
                    if (customAttributesStructure.TryGetValue(key, out var value))
                    {
                        customAttributes[key] = value!.AsObject!;
                    }
                }
            }

            UserContext? user = userContext == null ? null : new UserContext
            {
                id = userContext.ContainsKey("Id") ? userContext.GetValue("Id").AsString : null,
                name = userContext.ContainsKey("Name") ? userContext.GetValue("Name").AsString : null,
                email = userContext.ContainsKey("Email") ? userContext.GetValue("Email").AsString : null,
            };

            Dictionary<string, object> userCustomAttributes = new Dictionary<string, object>();

            if (userContext != null && userContext.TryGetValue("CustomAttributes", out var userCustomAttributesValue) &&
            userCustomAttributesValue?.AsStructure is Structure userCustomAttributesStructure)
            {
                foreach (var key in userCustomAttributesStructure.Keys)
                {
                    if (userCustomAttributesStructure.TryGetValue(key, out var value))
                    {
                        userCustomAttributes[key] = value!.AsObject!;
                    }
                }
            }
            
            if (user != null)
                user.customAttributes = userCustomAttributes;

            HyphenEvaluationContext payload = new HyphenEvaluationContext
            {
                targetingKey = context.TargetingKey!,
                ipAddress = ipAddress,
                user = user,
                application = application!,
                environment = environment!,
                customAttributes = customAttributes,
            };
            
            return payload;
        }
    }

    public class TelemetryPayload
    {
        public required HyphenEvaluationContext context { get; set; }
        public required TelemetryData data { get; set; }
    }

    public class TelemetryData
    {
        public required Evaluation toggle { get; set; }
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
