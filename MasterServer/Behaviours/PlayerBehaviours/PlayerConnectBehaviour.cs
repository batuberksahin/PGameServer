using System.Net;
using System.Net.Sockets;
using NetworkLibrary.Behaviours;

namespace MasterServer.Behaviours.PlayerBehaviours;

public class ConnectRequest
{
    public string? Username;
    public string? IpAddress;
}

public class ConnectResponse
{
    public bool? Success;
    public string? Message;

    public Guid PlayerId;
}

public class PlayerConnectBehaviour : BehaviourBase<ConnectRequest, ConnectResponse>

{
    public override Task<ConnectResponse> ExecuteBehaviourAsync(TcpClient client, ConnectRequest request)
    {
        Console.WriteLine("[$] Player " + request.Username + " connected");

        // TODO: Find or create player guid and return it to player
        
        var response = new ConnectResponse
        {
            Success = true,
            Message = "Connected successfully"
        };

        return Task.FromResult(response);
    }
}