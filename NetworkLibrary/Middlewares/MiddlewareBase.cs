using System.Net.Sockets;

namespace NetworkLibrary.Middlewares;

public abstract class MiddlewareBase<TRequest, TResponse>
{
  protected MiddlewareBase<TRequest, TResponse> NextMiddleware { get; private set; }

  public MiddlewareBase<TRequest, TResponse> SetNext(MiddlewareBase<TRequest, TResponse> nextMiddleware)
  {
    NextMiddleware = nextMiddleware;
    return nextMiddleware;
  }

  public abstract Task<bool> ProcessRequestAsync(TcpClient client, TRequest request, out TResponse response);
}