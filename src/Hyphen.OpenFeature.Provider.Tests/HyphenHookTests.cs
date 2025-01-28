using OpenFeature.Model;
using OpenFeature.Constant;

namespace Hyphen.OpenFeature.Provider.Tests;

public class HyphenHookTests
{
    private readonly string _testPublicKey = "public_dGVzdC1vcmc6dGVzdC1rZXk=";
    private readonly HyphenProviderOptions _defaultOptions = new()
    {
        Application = "test-app",
        Environment = "test-env",
        EnableToggleUsage = true
    };
    private readonly EvaluationContext _defaultContext = EvaluationContext.Builder()
        .SetTargetingKey("test-user")
        .Build();
    private readonly ClientMetadata _clientMetadata = new("test-client", "1.0.0");
    private readonly Metadata _providerMetadata = new("hyphen-toggle-dotnet");

    [Fact]
    public async Task AfterAsync_WithToggleUsageDisabled_SkipsTelemetry()
    {
        var options = new HyphenProviderOptions
        {
            Application = "test-app",
            Environment = "test-env",
            EnableToggleUsage = false
        };
        var hook = new HyphenHook(_testPublicKey, options);
        var flagKey = "test-flag";
        var value = new Value(true);
        var metadata = new Dictionary<string, object> { { "type", "boolean" } };

        var hookContext = new HookContext<Value>(flagKey, value, FlagValueType.Boolean, _clientMetadata, _providerMetadata, _defaultContext);
        var details = new FlagEvaluationDetails<Value>(
            flagKey,
            value,
            ErrorType.None,
            "test reason",
            null,
            null,
            new ImmutableMetadata(metadata)
        );

        await hook.AfterAsync(hookContext, details);
    }

}
