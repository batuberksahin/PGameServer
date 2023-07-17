using System.Net.Sockets;

namespace NetworkLibrary.Behaviours;

public interface IBehaviour
{
  Task ProcessRequestAsync(TcpClient client, string payload);
}