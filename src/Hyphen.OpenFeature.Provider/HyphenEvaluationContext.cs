namespace Hyphen.OpenFeature.Provider
{
    public class HyphenEvaluationContext
    {
        public required string TargetingKey { get; set; }
        public string? IpAddress { get; set; }
        public Dictionary<string, object>? CustomAttributes { get; set; }
        public UserContext? User { get; set; }
    }

    public class UserContext
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public Dictionary<string, object>? CustomAttributes { get; set; }
    }
}
