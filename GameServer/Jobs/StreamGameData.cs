using System.Net.Sockets;
using NetworkLibrary.Jobs;

namespace GameServer.Jobs;

public class StreamGameData : JobBase
{
    private List<TcpClient> _clients;

    public StreamGameData(List<TcpClient> clients)
    {
        _clients = clients;
    }

    public override Task RunAsync()
    {
        throw new NotImplementedException();
    }
}