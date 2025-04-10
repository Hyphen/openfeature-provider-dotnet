using System.Text;
using System.Text.Json;
using OpenFeature.Model;

namespace Hyphen.OpenFeature.Provider
{
    public class HyphenClient
    {
        private readonly HyphenProviderOptions options;
        private readonly Uri[] horizonUrls;
        private readonly Uri defaultHorizonUrl;
        private readonly CacheClient cache;
        private readonly HttpClient httpClient;

        public HyphenClient(string publicKey, HyphenProviderOptions options)
        {
            this.options = options;
            var orgId = GetOrgIdFromPublicKey(publicKey);
            defaultHorizonUrl = new Uri(!string.IsNullOrEmpty(orgId)
                ? $"https://{orgId}.toggle.hyphen.cloud"
                : "https://toggle.hyphen.cloud");
            var urlStrings = options.HorizonUrls ?? [];
            horizonUrls = [.. urlStrings.Select(u => new Uri(u)), defaultHorizonUrl];
            var cacheOptions = options.Cache ?? new CacheOptions();
            cache = new CacheClient(cacheOptions);
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", publicKey);
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
            var cachedResponse = cache.Get<EvaluationResponse>(context);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            ContextPayload payload = BuildPayloadFromContext(context);
            var response = await TryUrls("/toggle/evaluate", payload);
            var evaluationResponse = JsonSerializer.Deserialize<EvaluationResponse>(response);

            if (evaluationResponse != null)
            {
                cache.Set(context, evaluationResponse);
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
            foreach (var baseUrl in horizonUrls)
            {
                try
                {
                    var url = new Uri(baseUrl, urlPath.TrimStart('/'));
                    string? response = await HttpPost(url.ToString(), payload);
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

            var response = await httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed: {responseContent}");
            }

            return responseContent;
        }
        public ContextPayload BuildPayloadFromContext(EvaluationContext context)
        {
            string? application = context.ContainsKey("Application") ? context.GetValue("Application").AsString : options.Application;
            string? environment = context.ContainsKey("Environment") ? context.GetValue("Environment").AsString : options.Environment;
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
                        customAttributes[key] = HyphenUtils.ConvertValueToObject(value!)!;
                    }
                }
            }

            UserPayload? user = userContext == null ? null : new UserPayload
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
                        userCustomAttributes[key] = HyphenUtils.ConvertValueToObject(value!)!;
                    }
                }
            }

            if (user != null)
                user.customAttributes = userCustomAttributes;

            ContextPayload payload = new ContextPayload
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
        public required ContextPayload context { get; set; }
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

    public class ContextPayload
    {
        public required string targetingKey { get; set; }
        public required string application { get; set; }
        public required string environment { get; set; }
        public string? ipAddress { get; set; }
        public Dictionary<string, object>? customAttributes { get; set; }
        public UserPayload? user { get; set; }
    }

    public class UserPayload
    {
        public string? id { get; set; }
        public string? email { get; set; }
        public string? name { get; set; }
        public Dictionary<string, object>? customAttributes { get; set; }
    }
}
