using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using OpenFeature.Model;

namespace Hyphen.OpenFeature.Provider
{
    public class CacheClient
    {
        private readonly IMemoryCache _cache;
        private readonly Func<EvaluationContext, string> _generateCacheKeyFn;
        private readonly int _ttlSeconds;

        public CacheClient(CacheOptions options)
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _ttlSeconds = options?.TtlSeconds ?? 30;
            _generateCacheKeyFn = options?.GenerateCacheKeyFn ?? DefaultGenerateCacheKey;
        }

        private string DefaultGenerateCacheKey(EvaluationContext context)
        {
            var IpAddress = context.ContainsKey("IpAddress") ? context.GetValue("IpAddress").AsString : null;
            var CustomAttributes = context.ContainsKey("CustomAttributes") ? context.GetValue("CustomAttributes").AsStructure : null;
            var User = context.ContainsKey("User") ? context.GetValue("User").AsStructure : null;

            var normalizedContext = new
            {
                context.TargetingKey,
                IpAddress,
                CustomAttributes,
                User = User == null ? null : new
                {
                    Id = User.ContainsKey("Id") ? User.GetValue("Id").AsString : null,
                    Email = User.ContainsKey("Email") ? User.GetValue("Email").AsString : null,
                    Name = User.ContainsKey("Name") ? User.GetValue("Name").AsString : null,
                    CustomAttributes = User.ContainsKey("CustomAttributes") ? User.GetValue("CustomAttributes").AsStructure : null
                }
            };

            var jsonString = JsonSerializer.Serialize(normalizedContext);
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(jsonString));
            return Convert.ToBase64String(bytes);
        }

        public T? Get<T>(EvaluationContext context) where T : class
        {
            var key = _generateCacheKeyFn(context);
            return _cache.Get<T>(key);
        }

        public void Set<T>(EvaluationContext context, T value) where T : class
        {
            var key = _generateCacheKeyFn(context);
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(_ttlSeconds))
                .SetSlidingExpiration(TimeSpan.FromSeconds(_ttlSeconds / 2));
            
            _cache.Set(key, value, options);
        }
    }
}
