namespace DFC.App.JobProfile.MessageFunctionApp.HttpClientPolicies
{
    public class PolicyOptions
    {
        public CircuitBreakerPolicyOptions HttpCircuitBreaker { get; set; }

        public RetryPolicyOptions HttpRetry { get; set; }
    }
}