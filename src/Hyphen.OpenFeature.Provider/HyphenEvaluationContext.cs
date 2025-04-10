using OpenFeature.Model;

namespace Hyphen.OpenFeature.Provider
{
    public class HyphenEvaluationContext
    {
        /// <summary>
        /// The unique identifier for the user or entity being evaluated.
        /// </summary>
        public string? TargetingKey { get; init; }
        /// <summary>
        /// The IP address of the user making the request.
        /// </summary>
        public string? IpAddress { get; init; }

        /// <summary>
        /// Custom attributes for additional contextual information.
        /// </summary>
        public Dictionary<string, object>? CustomAttributes { get; init; }

        /// <summary>
        /// An object containing user-specific information for the evaluation.
        /// </summary>
        public UserContext? User { get; init; }

        /// <summary>
        /// Creates an instance of <see cref="EvaluationContext"/> with the specified properties.
        /// </summary>
        public EvaluationContext GetEvaluationContext()
        {
            EvaluationContextBuilder builder = EvaluationContext.Builder();
            if (!string.IsNullOrEmpty(IpAddress))
            {
                builder.Set("IpAddress", IpAddress);
            }
            if (CustomAttributes != null)
            {
                builder.Set("CustomAttributes", HyphenUtils.ConvertObjectToValue(CustomAttributes));
            }
            if (User != null)
            {
                builder.Set("User", HyphenUtils.ConvertObjectToValue(User));
                if (User.Id != null)
                {
                    builder.SetTargetingKey(User.Id.ToString());
                }
            }
            if (!string.IsNullOrEmpty(TargetingKey))
            {
                builder.SetTargetingKey(TargetingKey);
            }
            return builder.Build();
        }
    }

    public class UserContext
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// The name of the user.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Custom attributes specific to the user.
        /// </summary>
        public Dictionary<string, object>? CustomAttributes { get; set; }
    }
}
