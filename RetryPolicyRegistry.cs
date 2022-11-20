using Flurl.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace RetryHttpClient;

public class RetryPolicyRegistry : IRetryPolicyRegistry
{
	private readonly ILogger<RetryPolicyRegistry> _logger;

	public RetryPolicyRegistry(ILogger<RetryPolicyRegistry> logger)	=>
		_logger = logger;

	public AsyncRetryPolicy GetTransientRetryPolicy() =>
		Policy
			.Handle<FlurlHttpException>(IsWorthRetrying)
			.WaitAndRetryAsync(
				5,
				_ => TimeSpan.FromMilliseconds(1000),
				OnRetryAsyncFunc);

	private Task OnRetryAsyncFunc(Exception exception, TimeSpan timeSpan, int retryCount,
		Context context)
	{

		if (exception is FlurlHttpException httpException)
		{
			var requestBody = httpException.Call.RequestBody ?? "null";
			var requestUrl = httpException.Call.Request;
			var errorResponse = httpException.Call.Response;
			var responseBody = httpException.GetResponseStringAsync().GetAwaiter().GetResult() ?? "null";

			_logger.LogWarning(
				"Unable to execute, will retry in {TimeSpan}, attempt #{Attempt}. " +
				"Request URL: {RequestUrl}, " +
				"Request body: {RequestBody}, " +
				"Response: {ErrorResponse}, " +
				"Response Body: {ResponseBody}",
				timeSpan, retryCount, requestUrl, requestBody, errorResponse, responseBody);
		}
		else
		{
			_logger.LogWarning("Unable to execute , will retry in {TimeSpan}, attempt #{Attempt}",
				timeSpan, retryCount);
		}

		return Task.CompletedTask;
	}
	
	private static bool IsWorthRetrying(FlurlHttpException ex) {
		switch (ex.Call.Response.StatusCode) {
			case 408: // RequestTimeout
			case 502: // BadGateway
			case 503: // ServiceUnavailable
			case 504: // GatewayTimeout
				return true;
			default:
				return false;
		}
	}
}