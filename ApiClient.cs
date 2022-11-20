using Flurl.Http;
using Microsoft.Extensions.Logging;
using Polly;

namespace RetryHttpClient;

public class ApiClient : IApiClient
{
    private readonly IRetryPolicyRegistry _retryPolicyRegistry;
    private readonly ILogger<ApiClient> _logger;
    
    public ApiClient(IRetryPolicyRegistry retryPolicyRegistry, ILogger<ApiClient> logger)
    {
        _retryPolicyRegistry = retryPolicyRegistry;
        _logger = logger;
    }

    public async Task<string> GetAsync(string url)
    {
        _logger.LogInformation("Calling the API Get");
        
        var retry = _retryPolicyRegistry.GetTransientRetryPolicy();

        var policyResult = await retry.ExecuteAndCaptureAsync(async () => await url
            .GetStringAsync()
            .ConfigureAwait(false)).ConfigureAwait(false);

        return policyResult.Outcome == OutcomeType.Successful ? policyResult.Result : "Error";
    }
}