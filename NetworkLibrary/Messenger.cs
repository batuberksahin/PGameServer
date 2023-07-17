using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace NetworkLibrary;

public static class Messenger
{
  public static async Task SendResponseAsync(TcpClient client, string responseBehaviour, object response)
  {
    var json = JsonConvert.SerializeObject(response, new JsonSerializerSettings
                                                     {
                                                       TypeNameHandling = TypeNameHandling.Auto
                                                     });

    var finalString = $"{responseBehaviour}:{json}";

    var responseBytes = Encoding.UTF8.GetBytes(finalString);

    var stream = client.GetStream();
    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
  }
}