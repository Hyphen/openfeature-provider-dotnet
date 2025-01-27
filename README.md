# Hyphen Toggle OpenFeature Provider for .NET

The **Hyphen Toggle OpenFeature Provider** is an OpenFeature provider implementation for the Hyphen Toggle platform using .NET. It enables feature flag evaluation using the OpenFeature standard.

---

## Table of Contents

1. [Getting Started](#getting-started)
2. [Usage](#usage)
3. [Configuration](#configuration)
4. [Contributing](#contributing)
5. [License](#license)

---

## Getting Started

### Installation

Install the provider using NuGet:

```bash
dotnet add package Hyphen.OpenFeature.Provider
```

## Usage

### Example: Basic Setup

To integrate the Hyphen Toggle provider into your application, follow these steps:

1. **Set up the provider**: Register the `HyphenProvider` with OpenFeature using your `publicKey` and provider options.
2. **Evaluate a feature toggle**: Use the client to evaluate a feature flag.

```csharp
using OpenFeature;
using Hyphen.OpenFeature.Provider;

var publicKey = "your-public-key-here";
var options = new HyphenProviderOptions
{
    Application = "your-application-name",
    Environment = "production"
};

await OpenFeature.SetProviderAndWait(new HyphenProvider(publicKey, options));
var client = OpenFeature.GetClient();
var flagValue = await client.GetBooleanValue("feature-flag-key", false);
```

### Example: Contextual Evaluation

To evaluate a feature flag with specific user or application context, define and pass an `EvaluationContext`:

```csharp
var context = new HyphenEvaluationContext
{
    TargetingKey = "user-123",
    IpAddress = "203.0.113.42",
    CustomAttributes = new Dictionary<string, object>
    {
        { "subscriptionLevel", "premium" },
        { "region", "us-east" }
    },
    User = new UserContext
    {
        Id = "user-123",
        Email = "user@example.com",
        Name = "John Doe",
        CustomAttributes = new Dictionary<string, object>
        {
            { "role", "admin" }
        }
    }
};

var flagValue = await client.GetBooleanValue("feature-flag-key", false, context);
```

## Configuration

### Options

| Option              | Type      | Description                                     |
|--------------------|-----------|-------------------------------------------------|
| `Application`      | string    | The application id or alternate id              |
| `Environment`      | string    | The environment (e.g., `production`, `staging`) |
| `HorizonUrls`      | string[]  | Hyphen Horizon URLs for fetching flags         |
| `EnableToggleUsage`| bool?     | Enable/disable toggle usage logging            |
| `Cache`            | CacheOptions | Configuration for caching evaluations        |

### Cache Configuration

The `cache` option accepts the following properties:

| Property              | Type       | Default | Description                                                    |
|----------------------|------------|---------|----------------------------------------------------------------|
| `ttlSeconds`         | number     | 300     | Time-to-live in seconds for cached flag evaluations.           |
| `generateCacheKeyFn` | Function   | -       | Custom function to generate cache keys from evaluation context. |

Example with cache configuration:

```csharp
var options = new HyphenProviderOptions
{
    Application = "your-application-name",
    Environment = "production",
    Cache = new CacheOptions
    {
        TtlSeconds = 600, // 10 minutes
        GenerateCacheKeyFn = (context) => $"{context.TargetingKey}-{context.User?.Id}"
    }
};
```

### Context

Provide an `EvaluationContext` to pass contextual data for feature evaluation.

### Context Fields

| Field               | Type                           | Description                    |
|-------------------|--------------------------------|--------------------------------|
| `TargetingKey`    | string                         | Caching evaluation key        |
| `IpAddress`       | string                         | User's IP address             |
| `CustomAttributes`| Dictionary<string, object>     | Additional context info       |
| `User`           | UserContext                    | User-specific information     |

## Contributing

We welcome contributions to this project! If you'd like to contribute, please follow the guidelines outlined in [CONTRIBUTING.md](CONTRIBUTING.md). Whether it's reporting issues, suggesting new features, or submitting pull requests, your help is greatly appreciated!

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for full details.
