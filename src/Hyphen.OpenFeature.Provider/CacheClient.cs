using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace Hyphen.OpenFeature.Provider
{
    public class CacheClient
    {
        private readonly IMemoryCache _cache;
        private readonly Func<HyphenEvaluationContext, string> _generateCacheKeyFn;
        private readonly int _ttlSeconds;

        public CacheClient(CacheOptions options)
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _ttlSeconds = options?.TtlSeconds ?? 30;
            _generateCacheKeyFn = options?.GenerateCacheKeyFn ?? DefaultGenerateCacheKey;
        }

        private string DefaultGenerateCacheKey(HyphenEvaluationContext context)
        {
            var normalizedContext = new
            {
                context.TargetingKey,
                context.IpAddress,
                context.CustomAttributes,
                User = context.User == null ? null : new
                {
                    context.User.Id,
                    context.User.Email,
                    context.User.Name,
                    context.User.CustomAttributes
                }
            };

            var jsonString = JsonSerializer.Serialize(normalizedContext);
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(jsonString));
            return Convert.ToBase64String(bytes);
        }

        public T? Get<T>(HyphenEvaluationContext context) where T : class
        {
            var key = _generateCacheKeyFn(context);
            return _cache.Get<T>(key);
        }

        public void Set<T>(HyphenEvaluationContext context, T value) where T : class
        {
            var key = _generateCacheKeyFn(context);
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(_ttlSeconds))
                .SetSlidingExpiration(TimeSpan.FromSeconds(_ttlSeconds / 2));
            
            _cache.Set(key, value, options);
        }
    }
}
