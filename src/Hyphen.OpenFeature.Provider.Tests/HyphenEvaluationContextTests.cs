using OpenFeature.Model;
using System.Net;

namespace Hyphen.OpenFeature.Provider.Tests;

public class HyphenEvaluationContextTests
{
    [Fact]
    public void HyphenEvaluationContext_WithRequiredProperties_CreatesInstance()
    {
        var context = new HyphenEvaluationContext
        {
            targetingKey = "test-user",
            application = "test-app",
            environment = "test-env"
        };

        Assert.Equal("test-user", context.targetingKey);
        Assert.Equal("test-app", context.application);
        Assert.Equal("test-env", context.environment);
        Assert.Null(context.ipAddress);
        Assert.Null(context.customAttributes);
        Assert.Null(context.user);
    }

    [Fact]
    public void HyphenEvaluationContext_WithAllProperties_CreatesInstance()
    {
        var context = new HyphenEvaluationContext
        {
            targetingKey = "test-user",
            application = "test-app",
            environment = "test-env",
            ipAddress = IPAddress.Parse("127.0.0.1"),
            customAttributes = new Dictionary<string, object>
            {
                { "region", "us-east" }
            },
            user = new UserContext
            {
                id = "user-123",
                email = "test@example.com",
                name = "Test User",
                customAttributes = new Dictionary<string, object>
                {
                    { "role", "admin" }
                }
            }
        };

        Assert.Equal("test-user", context.targetingKey);
        Assert.Equal("test-app", context.application);
        Assert.Equal("test-env", context.environment);
        Assert.Equal(IPAddress.Parse("127.0.0.1"), context.ipAddress);
        Assert.NotNull(context.customAttributes);
        Assert.Single(context.customAttributes);
        Assert.Equal("us-east", context.customAttributes["region"]);

        Assert.NotNull(context.user);
        Assert.Equal("user-123", context.user.id);
        Assert.Equal("test@example.com", context.user.email);
        Assert.Equal("Test User", context.user.name);
        Assert.NotNull(context.user.customAttributes);
        Assert.Single(context.user.customAttributes);
        Assert.Equal("admin", context.user.customAttributes["role"]);
    }

    [Fact]
    public void UserContext_WithAllProperties_CreatesInstance()
    {
        var user = new UserContext
        {
            id = "user-123",
            email = "test@example.com",
            name = "Test User",
            customAttributes = new Dictionary<string, object>
            {
                { "role", "admin" },
                { "department", "engineering" }
            }
        };

        Assert.Equal("user-123", user.id);
        Assert.Equal("test@example.com", user.email);
        Assert.Equal("Test User", user.name);
        Assert.NotNull(user.customAttributes);
        Assert.Equal(2, user.customAttributes.Count);
        Assert.Equal("admin", user.customAttributes["role"]);
        Assert.Equal("engineering", user.customAttributes["department"]);
    }

    [Fact]
    public void UserContext_WithNullableProperties_CreatesInstance()
    {
        var user = new UserContext
        {
            id = "user-123"
        };

        Assert.Equal("user-123", user.id);
        Assert.Null(user.email);
        Assert.Null(user.name);
        Assert.Null(user.customAttributes);
    }

    [Fact]
    public void HyphenEvaluationContext_WithEmptyCustomAttributes_CreatesInstance()
    {
        var context = new HyphenEvaluationContext
        {
            targetingKey = "test-user",
            application = "test-app",
            environment = "test-env",
            customAttributes = new Dictionary<string, object>()
        };

        Assert.NotNull(context.customAttributes);
        Assert.Empty(context.customAttributes);
    }

    [Fact]
    public void UserContext_WithEmptyCustomAttributes_CreatesInstance()
    {
        var user = new UserContext
        {
            id = "user-123",
            customAttributes = new Dictionary<string, object>()
        };

        Assert.NotNull(user.customAttributes);
        Assert.Empty(user.customAttributes);
    }
}
