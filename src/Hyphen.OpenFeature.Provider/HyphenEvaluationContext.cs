namespace Hyphen.OpenFeature.Provider
{
    public class HyphenEvaluationContext
    {
        /// <summary>
        /// The key used for caching the evaluation response.
        /// </summary>
        public required string targetingKey { get; set; }
        
        /// <summary>
        /// The application name or ID for the current evaluation.
        /// </summary>
        public required string application { get; set; }
        
        /// <summary>
        /// The environment identifier for the Hyphen project.
        /// </summary>
        public required string environment { get; set; }
        
        /// <summary>
        /// The IP address of the user making the request.
        /// </summary>
        public string? ipAddress { get; set; }
        
        /// <summary>
        /// Custom attributes for additional contextual information.
        /// </summary>
        public Dictionary<string, object>? customAttributes { get; set; }
        
        /// <summary>
        /// An object containing user-specific information for the evaluation.
        /// </summary>
        public UserContext? user { get; set; }
    }

    public class UserContext
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public string? id { get; set; }
        
        /// <summary>
        /// The email address of the user.
        /// </summary>
        public string? email { get; set; }
        
        /// <summary>
        /// The name of the user.
        /// </summary>
        public string? name { get; set; }
        
        /// <summary>
        /// Custom attributes specific to the user.
        /// </summary>
        public Dictionary<string, object>? customAttributes { get; set; }
    }
}
