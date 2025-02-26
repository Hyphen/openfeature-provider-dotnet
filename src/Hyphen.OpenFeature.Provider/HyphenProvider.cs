using OpenFeature.Model;
using Metadata = OpenFeature.Model.Metadata;
using OpenFeature.Constant;
using OpenFeature;
using System.Text.Json;

namespace Hyphen.OpenFeature.Provider
{
    public class HyphenProvider(string publicKey, HyphenProviderOptions options) : FeatureProvider
    {
        private const string Name = "hyphen-toggle-dotnet";
        private readonly HyphenClient _hyphenClient = new(publicKey, options);
        private readonly Metadata _providerMetadata = new Metadata(Name);

        public override Task InitializeAsync(EvaluationContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            EvaluationContext _context = BuildContext(context);
            Api.Instance.AddHooks(new HyphenHook(publicKey, options));
            Api.Instance.SetContext(_context);
            return Task.CompletedTask;
        }
        public override Metadata GetMetadata()
        {
            return _providerMetadata;
        }

        public override async Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (context == null)
                    return new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.InvalidContext);
                Evaluation evaluation = await GetEvaluation<bool>(flagKey, context);
                if (evaluation.type != "boolean")
                    return new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.TypeMismatch);

                ImmutableMetadata metadata = GetMetadata(evaluation.type);

                return new ResolutionDetails<bool>(flagKey, Convert.ToBoolean(evaluation.value), ErrorType.None, evaluation.reason, null, null, metadata);
            }
            catch (Exception ex)
            {
                return new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.General, ex.Message);
            }
        }

        public override async Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (context == null)
                    return new ResolutionDetails<string>(flagKey, defaultValue, ErrorType.InvalidContext);

                Evaluation evaluation = await GetEvaluation<string>(flagKey, context);
                if (evaluation.type != "string" || evaluation.value == null)
                    return new ResolutionDetails<string>(flagKey, defaultValue, ErrorType.TypeMismatch);

                ImmutableMetadata metadata = GetMetadata(evaluation.type);
                return new ResolutionDetails<string>(flagKey, evaluation.value.ToString() ?? defaultValue, ErrorType.None, evaluation.reason, null, null, metadata);
            }
            catch (Exception ex)
            {
                return new ResolutionDetails<string>(flagKey, defaultValue, ErrorType.General, ex.Message);
            }
        }

        public override async Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (context == null)
                    return new ResolutionDetails<int>(flagKey, defaultValue, ErrorType.InvalidContext);
                Evaluation evaluation = await GetEvaluation<int>(flagKey, context);
                if (evaluation.type != "number")
                    return new ResolutionDetails<int>(flagKey, defaultValue, ErrorType.TypeMismatch);
                ImmutableMetadata metadata = GetMetadata(evaluation.type);
                return new ResolutionDetails<int>(flagKey, Convert.ToInt32(evaluation.value), ErrorType.None, evaluation.reason, null, null, metadata);
            }
            catch (Exception ex)
            {
                return new ResolutionDetails<int>(flagKey, defaultValue, ErrorType.General, ex.Message);
            }
        }
        public override async Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (context == null)
                    return new ResolutionDetails<double>(flagKey, defaultValue, ErrorType.InvalidContext);
                Evaluation evaluation = await GetEvaluation<double>(flagKey, context);
                if (evaluation.type != "number")
                    return new ResolutionDetails<double>(flagKey, defaultValue, ErrorType.TypeMismatch);

                ImmutableMetadata metadata = GetMetadata(evaluation.type);
                return new ResolutionDetails<double>(flagKey, Convert.ToDouble(evaluation.value), ErrorType.None, evaluation.reason, null, null, metadata);
            }
            catch (Exception ex)
            {
                return new ResolutionDetails<double>(flagKey, defaultValue, ErrorType.General, ex.Message);
            }
        }

        public override async Task<ResolutionDetails<Value>> ResolveStructureValueAsync(string flagKey, Value defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (context == null)
                    return new ResolutionDetails<Value>(flagKey, defaultValue, ErrorType.InvalidContext);
                Evaluation evaluation = await GetEvaluation<Value>(flagKey, context);

                if (evaluation.type != "object")
                    return new ResolutionDetails<Value>(flagKey, defaultValue, ErrorType.TypeMismatch);

                if (evaluation.value == null)
                    return new ResolutionDetails<Value>(flagKey, defaultValue, ErrorType.ParseError);

                ImmutableMetadata metadata = GetMetadata(evaluation.type);
                if (evaluation.value is JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == JsonValueKind.String)
                    {
                        try
                        {
                            var jsonString = jsonElement.GetString();

                            var parsedJson = JsonDocument.Parse(jsonString!).RootElement;
                            if (parsedJson.ValueKind == JsonValueKind.Object)
                            {
                                var structure = Structure.Builder();
                                foreach (var property in parsedJson.EnumerateObject())
                                {
                                    structure.Set(property.Name, property.Value.GetRawText());
                                }

                                return new ResolutionDetails<Value>(flagKey, new Value(structure.Build()), ErrorType.None, evaluation.reason, null, null, metadata);
                            }
                            else
                            {
                                return new ResolutionDetails<Value>(flagKey, defaultValue, ErrorType.TypeMismatch);
                            }
                        }
                        catch
                        {
                            return new ResolutionDetails<Value>(flagKey, defaultValue, ErrorType.ParseError);
                        }
                    }
                }

                return new ResolutionDetails<Value>(flagKey, new Value(evaluation.value), ErrorType.None, evaluation.reason, null, null, metadata);
            }
            catch (Exception ex)
            {
                return new ResolutionDetails<Value>(flagKey, defaultValue, ErrorType.General, ex.Message);
            }
        }
        private async Task<Evaluation> GetEvaluation<T>(string flagKey, EvaluationContext context)
        {
            EvaluationResponse response = await _hyphenClient.Evaluate(context);

            if (!response.toggles.TryGetValue(flagKey, out Evaluation? evaluation))
                throw new KeyNotFoundException($"Flag {flagKey} not found");


            T value;
            if (evaluation.value != null && evaluation.type != "object")
            {
                if (evaluation.value is JsonElement jsonElement)
                {
                    value = jsonElement.Deserialize<T>()!;
                }
                else
                {
                    value = (T)evaluation.value;
                }
                evaluation.value = value;
            }
            return evaluation;
        }
        private string GetTargetingKey(EvaluationContext context)
        {
            Structure? userContext = context.ContainsKey("User") ? context.GetValue("User").AsStructure : null;
            var user = userContext == null ? null : new
            {
                Id = userContext.ContainsKey("Id") ? userContext.GetValue("Id").AsString : null,
            };
            if (!string.IsNullOrEmpty(context.TargetingKey))
                return context.TargetingKey;
            if (user != null && !string.IsNullOrEmpty(user.Id))
                return user.Id;

            return $"{options.Application}-{options.Environment}-{Guid.NewGuid().ToString("N")[..8]}";
        }
        private EvaluationContext BuildContext(EvaluationContext context)
        {
            EvaluationContext _context = context;
            if (context == null)
            {
                _context = EvaluationContext.Builder().Build();
            }

            string targetingKey = GetTargetingKey(_context);
            string application = options.Application;
            string environment = options.Environment;

            EvaluationContextBuilder newContext = EvaluationContext.Builder()
                .Merge(_context)
                .Set("Application", new Value(application))
                .Set("Environment", new Value(environment))
                .SetTargetingKey(targetingKey);

            return newContext.Build();
        }

        private ImmutableMetadata GetMetadata(string type)
        {
            Dictionary<string, object> metadata = new Dictionary<string, object> { { "type", type } };
            return new ImmutableMetadata(metadata);
        }
    }
}
