using OpenFeature;
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

        private string GetTargetingKey(HyphenEvaluationContext context)
        {
            if (!string.IsNullOrEmpty(context.TargetingKey))
                return context.TargetingKey;
            if (context.User != null && !string.IsNullOrEmpty(context.User.Id))
                return context.User.Id;
            
            return $"{options.Application}-{options.Environment}-{Guid.NewGuid().ToString("N")[..8]}";
        }

        public async Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue, EvaluationContext context)
        {
            try
            {
                var evaluation = await GetEvaluation(flagKey, context);
                if (evaluation.Type != "boolean")
                    return new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.TypeMismatch);

                return new ResolutionDetails<bool>(flagKey, Convert.ToBoolean(evaluation.Value), ErrorType.None, evaluation.Reason);
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
                if (evaluation.Type != "string" || evaluation.Value == null)
                    return new ResolutionDetails<string>(flagKey, defaultValue, ErrorType.TypeMismatch);

                return new ResolutionDetails<string>(flagKey, evaluation.Value.ToString() ?? defaultValue, ErrorType.None, evaluation.Reason);
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
                if (evaluation.Type != "number")
                    return new ResolutionDetails<int>(flagKey, defaultValue, ErrorType.TypeMismatch);

                return new ResolutionDetails<int>(flagKey, Convert.ToInt32(evaluation.Value), ErrorType.None, evaluation.Reason);
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
                if (evaluation.Type != "object")
                    return new ResolutionDetails<T>(flagKey, defaultValue, ErrorType.TypeMismatch);

                var value = System.Text.Json.JsonSerializer.Deserialize<T>(evaluation.Value.ToString());
                return new ResolutionDetails<T>(flagKey, value, ErrorType.None, evaluation.Reason);
            }
            catch (Exception ex)
            {
                return new ResolutionDetails<T>(flagKey, defaultValue, ErrorType.General, ex.Message);
            }
        }

        private async Task<Evaluation> GetEvaluation(string flagKey, EvaluationContext context)
        {
            var hyphenContext = new HyphenEvaluationContext
            {
                TargetingKey = GetTargetingKey(context)
            };

            var response = await _hyphenClient.Evaluate(hyphenContext);
            if (!response.Toggles.TryGetValue(flagKey, out var evaluation))
                throw new KeyNotFoundException($"Flag {flagKey} not found");

            return evaluation;
        }
    }
}
