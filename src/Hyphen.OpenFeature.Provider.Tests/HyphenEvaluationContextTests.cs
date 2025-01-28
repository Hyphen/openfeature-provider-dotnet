namespace Hyphen.OpenFeature.Provider.Tests;

public class HyphenEvaluationContextTests
{
    [Theory]
    [InlineData("test-user", "test-app", "test-env")]
    public void HyphenEvaluationContext_WithRequiredProperties_CreatesInstance(string targetingKey, string application, string environment)
    {
        var context = new HyphenEvaluationContext
        {
            targetingKey = targetingKey,
            application = application,
            environment = environment
        };

        Assert.Equal(targetingKey, context.targetingKey);
        Assert.Equal(application, context.application);
        Assert.Equal(environment, context.environment);
        Assert.Null(context.ipAddress);
        Assert.Null(context.customAttributes);
        Assert.Null(context.user);
    }

    [Theory]
    [InlineData("test-user", "test-app", "test-env", "127.0.0.1", "us-east", "user-123", "test@example.com", "Test User", "admin")]
    public void HyphenEvaluationContext_WithAllProperties_CreatesInstance(
        string targetingKey, string application, string environment, 
        string ipAddressStr, string region,
        string userId, string userEmail, string userName, string userRole)
    {
        var context = new HyphenEvaluationContext
        {
            targetingKey = targetingKey,
            application = application,
            environment = environment,
            ipAddress = ipAddressStr,
            customAttributes = new Dictionary<string, object>
            {
                { "region", region }
            },
            user = new UserContext
            {
                id = userId,
                email = userEmail,
                name = userName,
                customAttributes = new Dictionary<string, object>
                {
                    { "role", userRole }
                }
            }
        };

        Assert.Equal(targetingKey, context.targetingKey);
        Assert.Equal(application, context.application);
        Assert.Equal(environment, context.environment);
        Assert.Equal(ipAddressStr, context.ipAddress);
        Assert.NotNull(context.customAttributes);
        Assert.Single(context.customAttributes);
        Assert.Equal(region, context.customAttributes["region"]);

        Assert.NotNull(context.user);
        Assert.Equal(userId, context.user.id);
        Assert.Equal(userEmail, context.user.email);
        Assert.Equal(userName, context.user.name);
        Assert.NotNull(context.user.customAttributes);
        Assert.Single(context.user.customAttributes);
        Assert.Equal(userRole, context.user.customAttributes["role"]);
    }

    [Theory]
    [InlineData("user-123", "test@example.com", "Test User", "admin", "engineering")]
    public void UserContext_WithAllProperties_CreatesInstance(
        string id, string email, string name, string role, string department)
    {
        var user = new UserContext
        {
            id = id,
            email = email,
            name = name,
            customAttributes = new Dictionary<string, object>
            {
                { "role", role },
                { "department", department }
            }
        };

        Assert.Equal(id, user.id);
        Assert.Equal(email, user.email);
        Assert.Equal(name, user.name);
        Assert.NotNull(user.customAttributes);
        Assert.Equal(2, user.customAttributes.Count);
        Assert.Equal(role, user.customAttributes["role"]);
        Assert.Equal(department, user.customAttributes["department"]);
    }

    [Theory]
    [InlineData("user-123")]
    public void UserContext_WithNullableProperties_CreatesInstance(string id)
    {
        var user = new UserContext
        {
            id = id
        };

        Assert.Equal(id, user.id);
        Assert.Null(user.email);
        Assert.Null(user.name);
        Assert.Null(user.customAttributes);
    }

    [Theory]
    [InlineData("test-user", "test-app", "test-env")]
    public void HyphenEvaluationContext_WithEmptyCustomAttributes_CreatesInstance(
        string targetingKey, string application, string environment)
    {
        var context = new HyphenEvaluationContext
        {
            targetingKey = targetingKey,
            application = application,
            environment = environment,
            customAttributes = new Dictionary<string, object>()
        };

        Assert.NotNull(context.customAttributes);
        Assert.Empty(context.customAttributes);
    }

    [Theory]
    [InlineData("user-123")]
    public void UserContext_WithEmptyCustomAttributes_CreatesInstance(string id)
    {
        var user = new UserContext
        {
            id = id,
            customAttributes = new Dictionary<string, object>()
        };

        Assert.NotNull(user.customAttributes);
        Assert.Empty(user.customAttributes);
    }
}
