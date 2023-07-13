using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetworkLibrary;
using NetworkLibrary.Jobs;

namespace GameServer.Jobs;

public class PingRequest
{
    public Guid? ServerId;
    public int? ServerPort;
    public long? Timestamp;
}

public class OpenRequest
{
    public Guid ServerId;
    public string? ServerIpAddress;
    public int? ServerPort;
    public long? Timestamp;
}

public class PingJob : JobBase
{
    private readonly string _ipAddress;
    private readonly int _port;
    
    private readonly TcpClient _tcpClient;

    private bool _isStarted;
    
    public PingJob(TcpClient tcpClient, string ipAddress, int port)
    {
        _tcpClient = tcpClient;
        
        _ipAddress = ipAddress;
        _port = port;
        
        _isStarted = false;
    }
    
    public override async Task RunAsync()
    {
        try
        {
            if (!_tcpClient.Connected)
                await _tcpClient.ConnectAsync(_ipAddress, _port);

            if (_isStarted)
            {
                var pingRequest = new PingRequest
                {
                    ServerId = GameServer.ServerId,
                    ServerPort = GameServer.Port,
                    Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()
                };
                
                await Messenger.SendResponseAsync(_tcpClient, "server_ping", pingRequest);

                Task.Run((() => HandleResponseAsync(_tcpClient)));
            }
            else
            {
                var openRequest = new OpenRequest
                {
                    ServerId = GameServer.ServerId,
                    ServerIpAddress = Dns.Resolve("localhost").AddressList[1].ToString(),
                    ServerPort = GameServer.Port,
                    Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()
                };

                await Messenger.SendResponseAsync(_tcpClient, "server_open", openRequest);
                
                Task.Run((() => HandleResponseAsync(_tcpClient)));

                _isStarted = true;
            }
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
        {
            Console.WriteLine("Connection to MasterServer refused.");
        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine("TcpClient has been disposed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending ping request: {ex.Message} {ex.Source}");
        }
    }

    private async Task HandleResponseAsync(TcpClient tcpClient)
    {
        var response = await ReadResponseAsync(tcpClient);

        Console.WriteLine(response);
    }

    private async Task<string> ReadResponseAsync(TcpClient tcpClient)
    {
        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        
        return response;
    }
}