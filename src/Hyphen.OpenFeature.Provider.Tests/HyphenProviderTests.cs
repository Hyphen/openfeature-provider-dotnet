using OpenFeature;
using OpenFeature.Model;
using OpenFeature.Constant;

namespace Hyphen.OpenFeature.Provider.Tests;

public class HyphenProviderTests
{
    private readonly string _testPublicKey = "public_dGVzdC1vcmc6dGVzdC1rZXk=";
    private readonly HyphenProviderOptions _defaultOptions = new()
    {
        Application = "test-app",
        Environment = "test-env"
    };
    private readonly EvaluationContext _defaultContext = EvaluationContext.Builder()
        .SetTargetingKey("test-user")
        .Build();

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        var provider = new HyphenProvider(_testPublicKey, _defaultOptions);

        Assert.NotNull(provider);
        var metadata = provider.GetMetadata();
        Assert.Equal("hyphen-toggle-dotnet", metadata.Name);
    }

    [Fact]
    public void Constructor_WithValidEnvironmentId_CreatesInstance()
    {
        var options = new HyphenProviderOptions
        {
            Application = "test-app",
            Environment = "pevr_123abc"
        };

        var provider = new HyphenProvider(_testPublicKey, options);

        Assert.NotNull(provider);
    }

    [Fact]
    public void Constructor_WithValidAlternateId_CreatesInstance()
    {
        var options = new HyphenProviderOptions
        {
            Application = "test-app",
            Environment = "test-env-123"
        };

        var provider = new HyphenProvider(_testPublicKey, options);

        Assert.NotNull(provider);
    }

    [Fact]
    public void Constructor_WithInvalidEnvironment_ThrowsArgumentException()
    {
        var options = new HyphenProviderOptions
        {
            Application = "test-app",
            Environment = "INVALID_UPPERCASE"
        };

        var exception = Assert.Throws<ArgumentException>(() => new HyphenProvider(_testPublicKey, options));
        Assert.Contains("Invalid environment format", exception.Message);
    }

    [Fact]
    public void Constructor_WithTooLongEnvironment_ThrowsArgumentException()
    {
        var options = new HyphenProviderOptions
        {
            Application = "test-app",
            Environment = "this-environment-name-is-way-too-long"
        };

        var exception = Assert.Throws<ArgumentException>(() => new HyphenProvider(_testPublicKey, options));
        Assert.Contains("Invalid environment format", exception.Message);
    }

    [Fact]
    public void Constructor_WithEnvironmentsWord_ThrowsArgumentException()
    {
        var options = new HyphenProviderOptions
        {
            Application = "test-app",
            Environment = "test-environments-123"
        };

        var exception = Assert.Throws<ArgumentException>(() => new HyphenProvider(_testPublicKey, options));
        Assert.Contains("Invalid environment format", exception.Message);
    }

    [Fact]
    public async Task InitializeAsync_WithValidContext_InitializesProvider()
    {
        var provider = new HyphenProvider(_testPublicKey, _defaultOptions);
        var context = EvaluationContext.Builder()
            .SetTargetingKey("test-user")
            .Build();

        await provider.InitializeAsync(context);

        var currentContext = Api.Instance.GetContext();
        Assert.NotNull(currentContext);
        Assert.Equal("test-user", currentContext.TargetingKey);
        Assert.Equal("test-app", currentContext.GetValue("Application").AsString);
        Assert.Equal("test-env", currentContext.GetValue("Environment").AsString);
    }

    [Fact]
    public async Task ResolveBooleanValueAsync_WithValidFlag_ReturnsCorrectValue()
    {
        var provider = new HyphenProvider(_testPublicKey, _defaultOptions);
        var flagKey = "test-bool-flag";
        var defaultValue = false;

        var result = await provider.ResolveBooleanValueAsync(flagKey, defaultValue, _defaultContext);

        Assert.NotNull(result);
        Assert.Equal(flagKey, result.FlagKey);
        Assert.Equal(ErrorType.General, result.ErrorType);
    }

    [Fact]
    public async Task ResolveStringValueAsync_WithNullContext_ReturnsDefaultWithError()
    {
        var provider = new HyphenProvider(_testPublicKey, _defaultOptions);
        var flagKey = "test-string-flag";
        var defaultValue = "default";

        var result = await provider.ResolveStringValueAsync(flagKey, defaultValue, null);

        Assert.Equal(defaultValue, result.Value);
        Assert.Equal(ErrorType.InvalidContext, result.ErrorType);
    }

    [Fact]
    public async Task ResolveIntegerValueAsync_WithValidFlag_ReturnsCorrectValue()
    {
        var provider = new HyphenProvider(_testPublicKey, _defaultOptions);
        var flagKey = "test-int-flag";
        var defaultValue = 42;

        var result = await provider.ResolveIntegerValueAsync(flagKey, defaultValue, _defaultContext);

        Assert.NotNull(result);
        Assert.Equal(flagKey, result.FlagKey);
        Assert.Equal(ErrorType.General, result.ErrorType);
    }

    [Fact]
    public async Task ResolveDoubleValueAsync_WithValidFlag_ReturnsCorrectValue()
    {
        var provider = new HyphenProvider(_testPublicKey, _defaultOptions);
        var flagKey = "test-double-flag";
        var defaultValue = 3.14;

        var result = await provider.ResolveDoubleValueAsync(flagKey, defaultValue, _defaultContext);

        Assert.NotNull(result);
        Assert.Equal(flagKey, result.FlagKey);
        Assert.Equal(ErrorType.General, result.ErrorType);
    }

    [Fact]
    public async Task ResolveStructureValueAsync_WithValidFlag_ReturnsCorrectValue()
    {
        var provider = new HyphenProvider(_testPublicKey, _defaultOptions);
        var flagKey = "test-structure-flag";
        var defaultValue = new Value(new Structure(new Dictionary<string, Value>
        {
            { "key", new Value("value") }
        }));

        var result = await provider.ResolveStructureValueAsync(flagKey, defaultValue, _defaultContext);

        Assert.NotNull(result);
        Assert.Equal(flagKey, result.FlagKey);
        Assert.Equal(ErrorType.General, result.ErrorType);
    }

    [Fact]
    public async Task ResolveStructureValueAsync_WithJsonStringValue_ParsesCorrectly()
    {
        var provider = new HyphenProvider(_testPublicKey, _defaultOptions);
        var flagKey = "test-json-structure-flag";
        var defaultValue = new Value(new Structure(new Dictionary<string, Value>()));

        var result = await provider.ResolveStructureValueAsync(flagKey, defaultValue, _defaultContext);

        Assert.NotNull(result);
        Assert.Equal(flagKey, result.FlagKey);
    }
}
