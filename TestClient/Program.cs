using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TestClient;

static class TestClient
{
    private static async Task Main(string[] args)
    {
        string userID = Console.ReadLine();
        
        var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888);

        using TcpClient client = new();
        await client.ConnectAsync(ipEndPoint);
        
        // Send server a message
        await using NetworkStream stream = client.GetStream();
        var message = $"player_connect:{userID}";
        Console.WriteLine($"{Dns.Resolve("localhost").AddressList[1].ToString()} sent message: {message}");
        var dateTimeBytes = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(dateTimeBytes);

        await Task.Delay(3000);

        Console.WriteLine("continue...");

        message = $"player_game_request:{userID}";
        Console.WriteLine($"{Dns.Resolve("localhost").AddressList[1].ToString()} sent message: {message}");
        dateTimeBytes = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(dateTimeBytes);
        
        // write if stream come from tcp client
        await using var streamasd = client.GetStream();
        var bytes = new byte[1024];
        var bytesRead = await streamasd.ReadAsync(bytes, 0, bytes.Length);
        var response = Encoding.ASCII.GetString(bytes, 0, bytesRead);
        Console.WriteLine($"Received: {response}");

        string? _ = Console.ReadLine();
    }
}