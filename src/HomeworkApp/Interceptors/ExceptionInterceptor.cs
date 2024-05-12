using Grpc.Core;
using Grpc.Core.Interceptors;
using HomeworkApp.Bll.Exceptions;
using System;
using System.Threading.Tasks;

namespace HomeworkApp.Interceptors;
public class ExceptionInterceptor : Interceptor
{
    public ExceptionInterceptor()
    {
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            var response = await continuation(request, context);
            return response;
        }
        catch (TooManyRequestsException ex)
        {
            throw new RpcException(new Status(StatusCode.Aborted, ex.Message));
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Unknown, ex.Message));
        }
    }
}
