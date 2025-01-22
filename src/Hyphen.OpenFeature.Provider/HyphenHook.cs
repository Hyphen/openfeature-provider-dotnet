using OpenFeature.Model;
using OpenFeature;
using System.Text.Json;

namespace Hyphen.OpenFeature.Provider
{
    public class HyphenHook(string publicKey, HyphenProviderOptions options) : Hook
    {
        public override async ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            if (options.EnableToggleUsage == false)
            {
                return;
            }
            HyphenClient hyphenClient = new(publicKey, options);
            HyphenEvaluationContext payloadFromContext = hyphenClient.BuildPayloadFromContext(context.EvaluationContext);
            string type = details.FlagMetadata?.GetString("type") ?? details.Value!.GetType().Name;

            Evaluation evaluationDetails = new Evaluation
            {
                key = details.FlagKey,
                value = details.Value!,
                type = type,
                reason = details.Reason,
            };
            HyphenEvaluationContext contextData = new HyphenEvaluationContext
            {
                targetingKey = payloadFromContext.targetingKey,
                application = payloadFromContext.application,
                environment = payloadFromContext.environment,
                ipAddress = payloadFromContext.ipAddress,
                user = payloadFromContext.user,
                customAttributes = payloadFromContext.customAttributes,
            };
            TelemetryPayload payload = new TelemetryPayload
            {
                context = contextData,
                data = new TelemetryData { toggle = evaluationDetails },
            };
            await hyphenClient.PostTelemetry(payload);

        }
    }
}