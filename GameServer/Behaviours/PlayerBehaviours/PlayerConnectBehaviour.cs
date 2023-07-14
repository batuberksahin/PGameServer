using System.Net.Sockets;
using NetworkLibrary.Behaviours;
using RepositoryLibrary.Models;

namespace GameServer.Behaviours.PlayerBehaviours;

public class ConnectRequest
{
    public Guid PlayerId;
    
    public Guid RoomId;
}

public class ConnectResponse
{
    public bool? Success;
    public string? Message;

    public Guid? RoomId;
    public List<Player>? Players;
}

public class PlayerConnectBehaviour : BehaviourBase<ConnectRequest, ConnectResponse>
{
    public override Task<ConnectResponse> ExecuteBehaviourAsync(TcpClient client, ConnectRequest request)
    {
        throw new NotImplementedException();
    }
}