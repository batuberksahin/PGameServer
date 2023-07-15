using System.Net.Sockets;
using System.Text;
using NetworkLibrary.Middlewares;
using Newtonsoft.Json;

namespace NetworkLibrary.Behaviours;

public abstract class BehaviourBase<TRequest, TResponse> : IBehaviour
{
    private readonly List<MiddlewareBase<TRequest, TResponse>> _middlewares;

    private readonly JsonSerializerSettings _jsonSerializerSettings;

    protected BehaviourBase(params MiddlewareBase<TRequest, TResponse>[] middlewares)
    {
        _middlewares = new List<MiddlewareBase<TRequest, TResponse>>(middlewares);
        
        _jsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
    }
    
    public async Task ProcessRequestAsync(TcpClient client, string payload)
    {
        TRequest request = DeserializeRequest(payload);
        TResponse response = default;
        
        foreach (var middleware in _middlewares)
        {
            bool middlewareResult = await middleware.ProcessRequestAsync(client, request, out response);
            if (middlewareResult) continue;
            
            await Console.Error.WriteLineAsync("Behavior blocked by middleware.");
            return;
        }

        TResponse executionResult = await ExecuteBehaviourAsync(client, request);
        if (executionResult != null)
        {
            response = executionResult;
        }

        if (response != null) await SendResponseAsync(client, response);
    }
    
    public abstract Task<TResponse> ExecuteBehaviourAsync(TcpClient client, TRequest request);
    
    private async Task SendResponseAsync(TcpClient client, TResponse response)
    {
        byte[] responseBytes = SerializeResponse(response);
        
        NetworkStream stream = client.GetStream();
        await stream.WriteAsync(responseBytes, 0, responseBytes.Length); 
    }
    
    private TRequest DeserializeRequest(string payload)
    {
        return JsonConvert.DeserializeObject<TRequest>(payload, _jsonSerializerSettings);
    }

    private byte[] SerializeResponse(TResponse response)
    {
        string json = JsonConvert.SerializeObject(response, _jsonSerializerSettings);
        return Encoding.UTF8.GetBytes(json);
    }
}