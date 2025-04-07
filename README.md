# Hyphen Toggle OpenFeature Provider for .NET

The **Hyphen Toggle OpenFeature Provider** is an OpenFeature provider implementation for the Hyphen Toggle platform using .NET. It enables feature flag evaluation using the OpenFeature standard.

---

## Table of Contents

1. [Installation](#installation)
2. [Setup and Initialization](#setup-and-initialization)
3. [Usage](#usage)
4. [Configuration](#configuration)
5. [Contributing](#contributing)
6. [License](#license)

---

## Installation
Install the provider and OpenFeature using NuGet:

```bash
dotnet add package Hyphen.OpenFeature.Provider
dotnet add package OpenFeature
```

## Setup and Initialization
To integrate the Hyphen Toggle provider into your application, follow these steps:

1. Configure the provider with your `publicKey` and provider options.
2. Register the provider with OpenFeature.

```csharp
using OpenFeature;
using Hyphen.OpenFeature.Provider;

var publicKey = "your-public-key-here";

// Example with alternateId environment format
var options = new HyphenProviderOptions
{
    Application = "your-application-name",
    Environment = "production"  // Using alternateId format
};

// OR using project environment ID format
// var options = new HyphenProviderOptions
// {
//     Application = "your-application-name",
//     Environment = "pevr_abc123"  // Using project environment ID format
// };

await OpenFeature.SetProviderAsync(new HyphenProvider(publicKey, options));
```

3. Configure the context needed for feature targeting evaluations, incorporating user or application context.
```csharp
HyphenEvaluationContext hyphenEvaluationContext = new HyphenEvaluationContext
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

var context = hyphenEvaluationContext.GetEvaluationContext();
```

### Usage
### Evaluation Context Example

```csharp
var client = OpenFeature.GetClient();
var flagValue = await client.GetBooleanValue("feature-flag-key", false, context);
```

## Configuration
### Options

| Option              | Type      | Required | Description                                     |
|--------------------|-----------|----------|-------------------------------------------------|
| `Application`      | string    | Yes      | The application id or alternate id              |
| `Environment`      | string    | Yes      | The environment identifier for the Hyphen project (project environment ID or alternateId). |
| `HorizonUrls`      | string[]  | No       | Hyphen Horizon URLs for fetching flags         |
| `EnableToggleUsage`| bool?     | No       | Enable/disable telemetry (default: True).      |
| `Cache`            | CacheOptions | No       | Configuration for caching evaluations        |

### Cache Configuration

The `cache` option accepts the following properties:

| Property              | Type       | Default | Description                                                    |
|----------------------|------------|---------|----------------------------------------------------------------|
| `TtlSeconds`         | number     | 300     | Time-to-live in seconds for cached flag evaluations.           |
| `GenerateCacheKeyFn` | Function   | -       | Custom function to generate cache keys from evaluation context. |

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

| Field               | Type                           | Required | Description                    |
|-------------------|--------------------------------|----------|--------------------------------|
| `TargetingKey`    | string                         | Yes      | Caching evaluation key        |
| `IpAddress`       | string                         | No       | User's IP address             |
| `CustomAttributes`| Dictionary<string, object>     | No       | Additional context information |
| `User`            | UserContext                    | No       | User-specific information     |
| `User.Id`         | string                         | No       | Unique identifier of the user |
| `User.Email`      | string                         | No       | Email address of the user |
| `User.Name`       | string                         | No       | Name of the user |
| `User.CustomAttributes` | Dictionary<string, object>  | No       | Custom attributes specific to the user |


## Contributing
We welcome contributions to this project! If you'd like to contribute, please follow the guidelines outlined in [CONTRIBUTING.md](CONTRIBUTING.md). Whether it's reporting issues, suggesting new features, or submitting pull requests, your help is greatly appreciated!

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for full details.
