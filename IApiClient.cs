namespace RetryHttpClient;

public interface IApiClient
{
    Task<string> GetAsync(string url);
}