using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Xunit;
using Assert = Xunit.Assert;

namespace TestClient;

public class UnitTest1
{
    [Fact]
    public async Task TestConnectBehavior()
    {
        // Arrange
        var ipAddress = "localhost";
        var port = 8888;

        using (var tcpClient = new TcpClient(ipAddress, port))
        {
            // Create the request object
            var request = new
            {
                Username = "12345",
                IpAddress = IPAddress.Any.ToString()
            };

            // Serialize the request object to JSON
            var requestJson = JsonConvert.SerializeObject(request);

            // Convert the request JSON to bytes
            var requestData = Encoding.UTF8.GetBytes("player_connect:" + requestJson);

            // Get the network stream of the TCP client
            var stream = tcpClient.GetStream();

            // Send the request bytes
            await stream.WriteAsync(requestData, 0, requestData.Length);
            
            // Assert
            // Add your assertions here to verify the behavior and response
            
            // Get the response bytes
            var responseData = new byte[1024];
            var bytesRead = await stream.ReadAsync(responseData, 0, responseData.Length);
            var responseJson = Encoding.UTF8.GetString(responseData, 0, bytesRead);
            
            // Deserialize the response JSON to an object
            var response = JsonConvert.DeserializeObject(responseJson);
            
            // Assert
            Assert.NotNull(response);

            // Cleanup
            stream.Close();
        }
    }

    [Fact]
    public async Task TestGameServerPingBehaviour()
    {
        // Arrange
        var ipAddress = "localhost";
        var port = 8888;

        var repeatCount = 30;

        var iteration = 0;
        
        await Task.Run(async () =>
        {
            while (iteration < repeatCount)
            {
                
                using (var tcpClient = new TcpClient(ipAddress, port))
                {
                    // Create the request object
                    var request = new
                    {
                        ServerId = "2068902a-d082-456b-a185-a064bb0182be",
                        ServerPort = "8889"
                    };

                    // Serialize the request object to JSON
                    var requestJson = JsonConvert.SerializeObject(request);

                    // Convert the request JSON to bytes
                    var requestData = Encoding.UTF8.GetBytes("server_ping:" + requestJson);

                    // Get the network stream of the TCP client
                    var stream = tcpClient.GetStream();

                    // Send the request bytes
                    await stream.WriteAsync(requestData, 0, requestData.Length);
            
                    // Assert
                    // Add your assertions here to verify the behavior and response
            
                    // Get the response bytes
                    var responseData = new byte[1024];
                    var bytesRead = await stream.ReadAsync(responseData, 0, responseData.Length);
                    var responseJson = Encoding.UTF8.GetString(responseData, 0, bytesRead);
            
                    // Deserialize the response JSON to an object
                    var response = JsonConvert.DeserializeObject(responseJson);
                }

                
                iteration++;
                
                Task.Delay(200);

            }
        });

        Assert.Equal(iteration, repeatCount);
    }
}
