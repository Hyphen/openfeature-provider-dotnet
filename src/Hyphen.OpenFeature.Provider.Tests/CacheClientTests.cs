using OpenFeature.Model;
using System.Text;

namespace Hyphen.OpenFeature.Provider.Tests;

public class CacheClientTests
{
    private readonly CacheOptions _defaultOptions = new()
    {
        TtlSeconds = 30
    };

    [Fact]
    public void Get_WithNonExistentKey_ReturnsNull()
    {
        var client = new CacheClient(_defaultOptions);
        var context = EvaluationContext.Builder()
            .SetTargetingKey("test-user")
            .Build();

        var result = client.Get<EvaluationResponse>(context);

        Assert.Null(result);
    }

    [Fact]
    public void SetAndGet_WithValidValue_ReturnsCachedValue()
    {
        var client = new CacheClient(_defaultOptions);
        var context = EvaluationContext.Builder()
            .SetTargetingKey("test-user")
            .Build();
        var value = new EvaluationResponse
        {
            toggles = new Dictionary<string, Evaluation>
            {
                { "test-flag", new Evaluation
                    {
                        key = "test-flag",
                        value = true,
                        type = "BOOLEAN"
                    }
                }
            }
        };

        client.Set(context, value);
        var result = client.Get<EvaluationResponse>(context);

        Assert.NotNull(result);
        Assert.NotNull(result);
        Assert.NotNull(result.toggles);
        Assert.Single(result.toggles);
        Assert.True(result.toggles.ContainsKey("test-flag"));
        Assert.Equal("BOOLEAN", result.toggles["test-flag"].type);
        Assert.True((bool)result.toggles["test-flag"].value);
    }

    [Fact]
    public void Get_WithDifferentContexts_ReturnsDifferentCacheKeys()
    {
        var client = new CacheClient(_defaultOptions);
        var context1 = EvaluationContext.Builder()
            .SetTargetingKey("user1")
            .Build();
        var context2 = EvaluationContext.Builder()
            .SetTargetingKey("user2")
            .Build();
        var value1 = new EvaluationResponse
        {
            toggles = new Dictionary<string, Evaluation>
            {
                { "flag1", new Evaluation { key = "flag1", value = true, type = "BOOLEAN" } }
            }
        };
        var value2 = new EvaluationResponse
        {
            toggles = new Dictionary<string, Evaluation>
            {
                { "flag2", new Evaluation { key = "flag2", value = false, type = "BOOLEAN" } }
            }
        };

        client.Set(context1, value1);
        client.Set(context2, value2);

        var result1 = client.Get<EvaluationResponse>(context1);
        var result2 = client.Get<EvaluationResponse>(context2);

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result1);
        Assert.NotNull(result1.toggles);
        Assert.NotNull(result2);
        Assert.NotNull(result2.toggles);
        Assert.True((bool)result1.toggles["flag1"].value);
        Assert.False((bool)result2.toggles["flag2"].value);
    }

    [Fact]
    public void Get_WithCustomCacheKeyGenerator_UsesCustomFunction()
    {
        var options = new CacheOptions
        {
            TtlSeconds = 30,
            GenerateCacheKeyFn = (context) => context.TargetingKey!
        };
        var client = new CacheClient(options);
        var context = EvaluationContext.Builder()
            .SetTargetingKey("test-user")
            .Build();
        var value = new EvaluationResponse
        {
            toggles = new Dictionary<string, Evaluation>
            {
                { "test-flag", new Evaluation { key = "test-flag", value = true, type = "BOOLEAN" } }
            }
        };

        client.Set(context, value);
        var result = client.Get<EvaluationResponse>(context);

        Assert.NotNull(result);
        Assert.NotNull(result);
        Assert.NotNull(result.toggles);
        Assert.True((bool)result.toggles["test-flag"].value);
    }
}
