using Polly.Retry;

namespace RetryHttpClient;

public interface IRetryPolicyRegistry
{
    AsyncRetryPolicy GetTransientRetryPolicy();
}