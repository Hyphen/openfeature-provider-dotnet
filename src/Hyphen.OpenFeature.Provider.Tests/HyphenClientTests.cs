using OpenFeature.Model;
using System.Net;
using System.Text;

namespace Hyphen.OpenFeature.Provider.Tests;

public class HyphenClientTests
{
    private readonly string _testPublicKey = "public_dGVzdC1vcmc6dGVzdC1rZXk=";
    private readonly HyphenProviderOptions _defaultOptions = new()
    {
        Application = "test-app",
        Environment = "test-env"
    };

    [Fact]
    public void Constructor_WithValidPublicKey_SetsCorrectHorizonUrl()
    {
        var client = new HyphenClient(_testPublicKey, _defaultOptions);

        Assert.NotNull(client);
    }

    [Fact]
    public void BuildPayloadFromContext_WithMinimalContext_ReturnsCorrectPayload()
    {
        var client = new HyphenClient(_testPublicKey, _defaultOptions);
        var context = EvaluationContext.Builder()
            .SetTargetingKey("test-user")
            .Build();

        var payload = client.BuildPayloadFromContext(context);

        Assert.Equal("test-user", payload.targetingKey);
        Assert.Equal(_defaultOptions.Application, payload.application);
        Assert.Equal(_defaultOptions.Environment, payload.environment);
        Assert.Null(payload.ipAddress);
        Assert.Null(payload.user);
    }

    [Fact]
    public void BuildPayloadFromContext_WithFullContext_ReturnsCorrectPayload()
    {
        var client = new HyphenClient(_testPublicKey, _defaultOptions);
        var userAttributes = new Dictionary<string, Value>
        {
            { "Id", new Value("user-123") },
            { "Name", new Value("Test User") },
            { "Email", new Value("test@example.com") },
            { "CustomAttributes", new Value(new Structure(new Dictionary<string, Value>
            {
                { "role", new Value("admin") }
            }))}
        };

        var context = EvaluationContext.Builder()
            .SetTargetingKey("test-user")
            .Set("User", new Value(new Structure(userAttributes)))
            .Set("IpAddress", new Value(IPAddress.Parse("127.0.0.1").ToString()))
            .Set("CustomAttributes", new Value(new Structure(new Dictionary<string, Value>
            {
                { "region", new Value("us-east") }
            })))
            .Build();

        var payload = client.BuildPayloadFromContext(context);

        Assert.Equal("test-user", payload.targetingKey);
        Assert.Equal(IPAddress.Parse("127.0.0.1").ToString(), payload.ipAddress?.ToString());
        Assert.NotNull(payload.user);
        Assert.Equal("user-123", payload.user.id);
        Assert.Equal("Test User", payload.user.name);
        Assert.Equal("test@example.com", payload.user.email);
        Assert.NotNull(payload.customAttributes);
        Assert.Single(payload.customAttributes!);
        Assert.Equal("us-east", payload.customAttributes!["region"]);
        Assert.Single(payload.user.customAttributes!);
        Assert.Equal("admin", payload.user.customAttributes!["role"]);
    }
}
