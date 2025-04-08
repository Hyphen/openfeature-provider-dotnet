using OpenFeature.Model;
using Hyphen.OpenFeature.Provider;
using OpenFeature;
using System.Text.Json;

HyphenProvider hx = new("publick-key", new HyphenProviderOptions
{
    Application = "application-name",
    Environment = "development",
    // Environment = "pevr_67ed807f195d4cfb93eacdc1",
    // EnableToggleUsage = false,
    Cache = new CacheOptions
    {
        TtlSeconds = 600,
        GenerateCacheKeyFn = (EvaluationContext ctx) => ctx.ContainsKey("IpAddress") ? ctx.GetValue("IpAddress").AsString : "default"
    }
});


HyphenEvaluationContext hyphenEvaluationContext = new HyphenEvaluationContext
{
    TargetingKey = "targeting-key",
    IpAddress = "127.0.0.1",
    User =
    new UserContext
    {
        Id = "user-id",
        Name = "John Doe",
        Email = "john@doe.com",
        CustomAttributes = new Dictionary<string, object>
        {
            { "NestedObject", new Dictionary<string, object>
                {
                    { "nestedBool", false },
                    { "nestedInt", 1 },
                    { "nestedDouble", 3.14 },
                    { "nestedList", new object[] { "item1", "item2", 99, true } },
                    { "nestedDate", new DateTime(2022, 1, 1) },
                    { "null", null }
                }
            },
            { "Key2", "uservalue2" }
        }
    },
    CustomAttributes = new Dictionary<string, object>
    {
        { "NestedObject", new Dictionary<string, object>
            {
                { "nestedBool", false },
                { "nestedInt", 1 },
                { "nestedDouble", 3.14 },
                { "nestedList", new object[] { "item1", "item2", 99, true } },
                { "nestedDate", new DateTime(2022, 1, 1) },
                { "null", null }
            }
        },
        { "Key2", "uservalue2" }
    }
};

var ctx = hyphenEvaluationContext.GetEvaluationContext();


await Api.Instance.SetProviderAsync(hx);
FeatureClient client = Api.Instance.GetClient();

bool boolVar = await client.GetBooleanValueAsync("boolean-test", false, ctx);
Console.WriteLine($"boolVar: {boolVar}");

string stringVar = await client.GetStringValueAsync("string-test", "default", ctx);
Console.WriteLine($"stringVar: {stringVar}");

int intVar = await client.GetIntegerValueAsync("int-test", 0, ctx);
Console.WriteLine($"intVar: {intVar}");

double doubleVar = await client.GetDoubleValueAsync("double-test", 0, ctx);
Console.WriteLine($"doubleVar: {doubleVar}");

Value valueVar = await client.GetObjectValueAsync("object-test", new Value(new Structure(new Dictionary<string, Value> { { "key", new Value("default value") } })), ctx);
Console.WriteLine($"keys: {JsonSerializer.Serialize(valueVar.AsStructure?.Keys)}");
Thread.Sleep(1000000);
