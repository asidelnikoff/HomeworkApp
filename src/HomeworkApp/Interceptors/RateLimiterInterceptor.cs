using Grpc.Core;
using Grpc.Core.Interceptors;
using HomeworkApp.Bll.Services.Interfaces;
using System.Threading.Tasks;

namespace HomeworkApp.Middlewares;

public class RateLimiterInterceptor : Interceptor
{
    private readonly IRateLimiterService _rateLimiterService;

    public RateLimiterInterceptor(IRateLimiterService rateLimiterService)
    {
        _rateLimiterService = rateLimiterService;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var clientName = context.RequestHeaders.Get("X-R256-USER-IP");
        if (clientName is null)
        {
            throw new RpcException(new Status(
                StatusCode.Unknown,
                "X-R256-USER-IP header was null"));
        }

        await _rateLimiterService.ThrowIfTooManyRequests(clientName.Value);

        return await continuation(request, context);
    }
}
