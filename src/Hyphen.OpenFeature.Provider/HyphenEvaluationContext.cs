using System.Net;

namespace Hyphen.OpenFeature.Provider
{
    public class HyphenEvaluationContext
    {
        public required string targetingKey { get; set; }
        public required string application { get; set; }
        public required string environment { get; set; }
        public IPAddress? ipAddress { get; set; }
        public Dictionary<string, object>? customAttributes { get; set; }
        public UserContext? user { get; set; }
    }

    public class UserContext
    {
        public string? id { get; set; }
        public string? email { get; set; }
        public string? name { get; set; }
        public Dictionary<string, object>? customAttributes { get; set; }
    }
}
