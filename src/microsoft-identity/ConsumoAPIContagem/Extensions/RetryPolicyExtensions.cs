using Polly;
using Polly.Retry;
using ConsumoAPIContagem.Models;

namespace ConsumoAPIContagem.Extensions;

public static class RetryPolicyExtensions
{
    public static async Task<T> ExecuteWithTokenAsync<T>(
        this AsyncRetryPolicy retryPolicy,
        Token token,
        Func<Context, Task<T>> action)
    {
        return await retryPolicy.ExecuteAsync(action,
            new Dictionary<string, object>
            {
                { "AccessToken", token.AccessToken! }
            });
    }
}