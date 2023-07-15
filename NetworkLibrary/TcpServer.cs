using System.Net;
using System.Net.Sockets;
using System.Text;
using NetworkLibrary.Behaviours;
using Newtonsoft.Json;

namespace NetworkLibrary;

public class TcpServer : ITcpServer
{
    private readonly TcpListener _tcpListener;
    private readonly BehaviourEngine _behaviourEngine;
    
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public TcpServer(short port)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);
        
        _tcpListener = new TcpListener(ipEndPoint);
        _behaviourEngine = new BehaviourEngine();
        
        _jsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
    }

    public async Task StartAsync()
    {
        try
        {
            _tcpListener.Start();
            
            while (true)
            {
                TcpClient client = await _tcpListener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleRequest(client));
            }
        } catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        }
    }

    public void StopImmediately()
    {
        try
        {
            _tcpListener.Stop();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        }
    }

    public void RegisterBehaviour(string behaviourName, IBehaviour behaviour)
    {
        _behaviourEngine.RegisterBehaviour(behaviourName, behaviour);
    }

    private async Task HandleRequest(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;
            
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                byte[] receivedBytes = new byte[bytesRead];
                Buffer.BlockCopy(buffer, 0, receivedBytes, 0, bytesRead);
                
                string data = Encoding.UTF8.GetString(receivedBytes);
                
                _ = Task.Run(() => _behaviourEngine.ParseRequestAsync(client, data));
            }
        }
        catch (IOException ex) when (ex.InnerException is SocketException socketException && socketException.SocketErrorCode == SocketError.ConnectionReset)
        {
            Console.WriteLine("Connection closed by remote host.");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
            
        }
    }
}