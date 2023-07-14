using System.Net.Sockets;
using NetworkLibrary.Behaviours;
using RepositoryLibrary.Models;

namespace GameServer.Behaviours.MasterServerBehaviours;

public class NewRoomRequest
{
    public List<Player>? Players;
}

public class NewRoomResponse
{
    public bool? Success;
    public string? Message;

    public Guid? RoomId;
}

public class NewRoomBehaviour : BehaviourBase<NewRoomRequest, NewRoomResponse>
{
    public override Task<NewRoomResponse> ExecuteBehaviourAsync(TcpClient client, NewRoomRequest request)
    {
        throw new NotImplementedException();
    }
}