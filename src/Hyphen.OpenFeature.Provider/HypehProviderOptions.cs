namespace Hyphen.OpenFeature.Provider
{
    public class HyphenProviderOptions
    {
        public string Application { get; set; }
        public string Environment { get; set; }
        public string[]? HorizonUrls { get; set; }
        public bool? EnableToggleUsage { get; set; } = true;
        public CacheOptions? Cache { get; set; }
    }

    public class CacheOptions
    {
        public int? TtlSeconds { get; set; } = 30;
        public Func<HyphenEvaluationContext, string>? GenerateCacheKeyFn { get; set; }
    }
}
