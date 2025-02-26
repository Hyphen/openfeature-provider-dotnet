using OpenFeature.Model;

namespace Hyphen.OpenFeature.Provider
{
    public class HyphenProviderOptions
    {
        /// <summary>
        /// The application name or ID for the current evaluation.
        /// </summary>
        public required string Application { get; set; }
        
        /// <summary>
        /// The environment identifier for the Hyphen project.
        /// This can be either:
        /// - A project environment ID (e.g., `pevr_abc123`)
        /// - A valid alternateId (1-25 characters, lowercase letters, numbers, hyphens, and underscores)
        /// </summary>
        public required string Environment { get; set; }
        
        /// <summary>
        /// The Hyphen server URL
        /// </summary>
        public string[]? HorizonUrls { get; set; }
        
        /// <summary>
        /// Flag to enable toggle usage
        /// </summary>
        public bool? EnableToggleUsage { get; set; } = true;
        
        /// <summary>
        /// The cache options for the provider
        /// </summary>
        public CacheOptions? Cache { get; set; }
    }

    public class CacheOptions
    {
        /// <summary>
        /// The time-to-live (TTL) in seconds for the cache.
        /// </summary>
        public int? TtlSeconds { get; set; } = 30;
        
        /// <summary>
        /// Generate a cache key function for the evaluation context.
        /// </summary>
        public Func<EvaluationContext, string>? GenerateCacheKeyFn { get; set; }
    }
}
