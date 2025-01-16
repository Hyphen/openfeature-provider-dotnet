using OpenFeature.Model;
using Metadata = OpenFeature.Model.Metadata;
using OpenFeature.Constant;

namespace Hyphen.OpenFeature.Provider
{
    public class HyphenProvider(string publicKey, HyphenProviderOptions options)
    {
        private const string Name = "hyphen-toggle-dotnet";
        private readonly HyphenClient _hyphenClient = new(publicKey, options);
        private readonly Metadata _providerMetadata = new Metadata(Name);

        private string GetTargetingKey(EvaluationContext context)
        {
            var UserContext = context.ContainsKey("User") ? context.GetValue("User").AsStructure : null;
            var User = UserContext == null ? null : new
            {
                Id = UserContext.ContainsKey("Id") ? UserContext.GetValue("Id").AsString : null,
            };
            if (!string.IsNullOrEmpty(context.TargetingKey))
                return context.TargetingKey;
            if (User != null && !string.IsNullOrEmpty(User.Id))
                return User.Id;

            return $"{options.Application}-{options.Environment}-{Guid.NewGuid().ToString("N")[..8]}";
        }

        public async Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue, EvaluationContext context)
        {
            try
            {
                var evaluation = await GetEvaluation(flagKey, context);
                if (evaluation.type != "boolean")
                    return new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.TypeMismatch);

                return new ResolutionDetails<bool>(flagKey, Convert.ToBoolean(evaluation.value), ErrorType.None, evaluation.reason);
            }
            catch (Exception ex)
            {
                return new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.General, ex.Message);
            }
        }

        public async Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue, EvaluationContext context)
        {
            try
            {
                var evaluation = await GetEvaluation(flagKey, context);
                if (evaluation.type != "string" || evaluation.value == null)
                    return new ResolutionDetails<string>(flagKey, defaultValue, ErrorType.TypeMismatch);

                return new ResolutionDetails<string>(flagKey, evaluation.value.ToString() ?? defaultValue, ErrorType.None, evaluation.reason);
            }
            catch (Exception ex)
            {
                return new ResolutionDetails<string>(flagKey, defaultValue, ErrorType.General, ex.Message);
            }
        }

        public async Task<ResolutionDetails<int>> ResolveNumberValue(string flagKey, int defaultValue, EvaluationContext context)
        {
            try
            {
                var evaluation = await GetEvaluation(flagKey, context);
                if (evaluation.type != "number")
                    return new ResolutionDetails<int>(flagKey, defaultValue, ErrorType.TypeMismatch);

                return new ResolutionDetails<int>(flagKey, Convert.ToInt32(evaluation.value), ErrorType.None, evaluation.reason);
            }
            catch (Exception ex)
            {
                return new ResolutionDetails<int>(flagKey, defaultValue, ErrorType.General, ex.Message);
            }
        }

        public async Task<ResolutionDetails<T>> ResolveStructureValue<T>(string flagKey, T defaultValue, EvaluationContext context)
        {
            try
            {
                var evaluation = await GetEvaluation(flagKey, context);

                if (evaluation.type != "object")
                    return new ResolutionDetails<T>(flagKey, defaultValue, ErrorType.TypeMismatch);

                if (evaluation.value == null)
                    return new ResolutionDetails<T>(flagKey, defaultValue, ErrorType.ParseError);

                var value = System.Text.Json.JsonSerializer.Deserialize<T>(evaluation.value.ToString()!);
                return new ResolutionDetails<T>(flagKey, value!, ErrorType.None, evaluation.reason);
            }
            catch (Exception ex)
            {
                return new ResolutionDetails<T>(flagKey, defaultValue, ErrorType.General, ex.Message);
            }
        }


        private async Task<Evaluation> GetEvaluation(string flagKey, EvaluationContext context)
        {
            var response = await _hyphenClient.Evaluate(context);
            if (!response.toggles.TryGetValue(flagKey, out var evaluation))
                throw new KeyNotFoundException($"Flag {flagKey} not found");

            return evaluation;
        }
    }
}
