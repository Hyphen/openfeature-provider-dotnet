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

        private void ValidateContext(EvaluationContext context)
        {
            var hyphenContext = new HyphenEvaluationContext
            {
                TargetingKey = context?.TargetingKey,
                User = context?.TargetingKey != null ? new User { Id = context.TargetingKey } : null
            };
            if (string.IsNullOrEmpty(hyphenContext.TargetingKey))
                throw new ArgumentException("targetingKey is required");
        }

        public async Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null)
        {
            try
            {
                var evaluation = await GetEvaluation(flagKey, context);
                if (evaluation.Type != "boolean")
                    return new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.TypeMismatch);

                return new ResolutionDetails<bool>
                {
                    
                    Value = Convert.ToBoolean(evaluation.Value),
                    Reason = evaluation.Reason
                };
            }
            catch (Exception ex)
            {
                return new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.General, ex.Message);
            }
        }

        public async Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue, EvaluationContext context = null)
        {
            try
            {
                var evaluation = await GetEvaluation(flagKey, context);
                if (evaluation.Type != "string")
                    return new ResolutionDetails<string>(flagKey, defaultValue, ErrorType.TypeMismatch);

                return new ResolutionDetails<string>
                {
                    Value = evaluation.Value.ToString(),
                    Reason = evaluation.Reason
                };
            }
            catch (Exception ex)
            {
                return new ResolutionDetails<string>(flagKey, defaultValue, ErrorType.General, ex.Message);
            }
        }

        public async Task<ResolutionDetails<int>> ResolveNumberValue(string flagKey, int defaultValue, EvaluationContext context = null)
        {
            try
            {
                var evaluation = await GetEvaluation(flagKey, context);
                if (evaluation.Type != "number")
                    return new ResolutionDetails<int>(flagKey, defaultValue, ErrorType.TypeMismatch);

                return new ResolutionDetails<int>
                {
                    Value = Convert.ToInt32(evaluation.Value),
                    Reason = evaluation.Reason
                };
            }
            catch (Exception ex)
            {
                return new ResolutionDetails<int>(flagKey, defaultValue, ErrorType.Error, ex.Message);
            }
        }

        public async Task<ResolutionDetails<T>> ResolveStructureValue<T>(string flagKey, T defaultValue, EvaluationContext context = null)
        {
            try
            {
                var evaluation = await GetEvaluation(flagKey, context);
                if (evaluation.Type != "object")
                    return new ResolutionDetails<T>(flagKey, defaultValue, ErrorType.TypeMismatch);

                var value = System.Text.Json.JsonSerializer.Deserialize<T>(evaluation.Value.ToString());
                return new ResolutionDetails<T>
                {
                    Value = value,
                    Reason = evaluation.Reason
                };
            }
            catch (Exception ex)
            {
                return new ResolutionDetails<T>(flagKey, defaultValue, ErrorType.General, ex.Message);
            }
        }

        private async Task<Evaluation> GetEvaluation(string flagKey, EvaluationContext context)
        {
            var hyphenContext = context as HyphenEvaluationContext ?? new HyphenEvaluationContext();
            hyphenContext.TargetingKey = GetTargetingKey(hyphenContext);
            ValidateContext(hyphenContext);

            var response = await _hyphenClient.Evaluate(hyphenContext);
            if (!response.Toggles.ContainsKey(flagKey))
                throw new KeyNotFoundException($"Flag {flagKey} not found");

            return response.Toggles[flagKey];
        }
    }
}
