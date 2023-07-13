using System.Net;
using System.Net.Sockets;
using NetworkLibrary.Behaviours;

namespace MasterServer.Behaviours.PlayerBehaviours;

public class ConnectRequest
{
    public string Username;
    public string IpAddress;
}

public class ConnectResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
}

public class PlayerConnectBehaviour : BehaviourBase<ConnectRequest, ConnectResponse>

{
    public override async Task<ConnectResponse> ExecuteBehaviourAsync(TcpClient client, ConnectRequest request)
    {
        Console.WriteLine("[$] Player " + request.Username + " connected");

        var response = new ConnectResponse
        {
            Success = true,
            Message = "Connected successfully"
        };

        return response;
    }
}