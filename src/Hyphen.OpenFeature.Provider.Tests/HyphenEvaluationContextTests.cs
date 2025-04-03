using System.Collections.Generic;
using OpenFeature.Model;
using Xunit;

namespace Hyphen.OpenFeature.Provider.Tests
{
    public class HyphenEvaluationContextTests
    {
        [Fact]
        public void GetEvaluationContext_WithIpAddress_SetsIpAddress()
        {
            var context = new HyphenEvaluationContext
            {
                IpAddress = "192.168.1.1"
            };

            var evaluationContext = context.GetEvaluationContext();

            Assert.Equal("192.168.1.1", evaluationContext.GetValue("IpAddress").AsString);
        }

        [Fact]
        public void GetEvaluationContext_WithCustomAttributes_SetsCustomAttributes()
        {
            var customAttributes = new Dictionary<string, object>
            {
                { "Attribute1", "Value1" },
                { "Attribute2", 123 }
            };
            var context = new HyphenEvaluationContext
            {
                CustomAttributes = customAttributes
            };

            var evaluationContext = context.GetEvaluationContext();

            var attributes = evaluationContext.GetValue("CustomAttributes").AsStructure;
            Assert.Equal("Value1", attributes!["Attribute1"].AsString);
            Assert.Equal(123, attributes["Attribute2"].AsInteger);
        }

        [Fact]
        public void GetEvaluationContext_WithUser_SetsUser()
        {
            var user = new UserContext
            {
                Id = "user123",
                Email = "user@example.com",
                Name = "Test User"
            };
            var context = new HyphenEvaluationContext
            {
                User = user
            };

            var evaluationContext = context.GetEvaluationContext();

            var userValue = evaluationContext.GetValue("User").AsStructure;
            Assert.Equal("user123", userValue!["Id"].AsString);
            Assert.Equal("user@example.com", userValue["Email"].AsString);
            Assert.Equal("Test User", userValue["Name"].AsString);
            Assert.Equal("user123", evaluationContext.TargetingKey);
        }

        [Fact]
        public void GetEvaluationContext_WithTargetingKey_SetsTargetingKey()
        {
            var context = new HyphenEvaluationContext
            {
                TargetingKey = "target123"
            };

            var evaluationContext = context.GetEvaluationContext();

            Assert.Equal("target123", evaluationContext.TargetingKey);
        }

        [Fact]
        public void GetEvaluationContext_WithAllProperties_SetsAllProperties()
        {
            var customAttributes = new Dictionary<string, object>
            {
                { "Attribute1", "Value1" },
                { "Attribute2", 123 }
            };
            var user = new UserContext
            {
                Id = "user123",
                Email = "user@example.com",
                Name = "Test User"
            };
            var context = new HyphenEvaluationContext
            {
                IpAddress = "192.168.1.1",
                CustomAttributes = customAttributes,
                User = user,
                TargetingKey = "target123"
            };

            var evaluationContext = context.GetEvaluationContext();

            Assert.Equal("192.168.1.1", evaluationContext.GetValue("IpAddress").AsString);
            var attributes = evaluationContext.GetValue("CustomAttributes").AsStructure;
            Assert.Equal("Value1", attributes!["Attribute1"].AsString);
            Assert.Equal(123, attributes["Attribute2"].AsInteger);
            var userValue = evaluationContext.GetValue("User").AsStructure;
            Assert.Equal("user123", userValue!["Id"].AsString);
            Assert.Equal("user@example.com", userValue["Email"].AsString);
            Assert.Equal("Test User", userValue["Name"].AsString);
            Assert.Equal("target123", evaluationContext.TargetingKey);
        }
    }
}
