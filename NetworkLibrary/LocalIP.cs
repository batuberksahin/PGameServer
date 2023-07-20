using System.Net;
using System.Net.Sockets;

namespace NetworkLibrary;

public class LocalIP
{
  public static string GetLocalIPv4()
  {
    try
    {
      string      hostName  = Dns.GetHostName();
      IPHostEntry hostEntry = Dns.GetHostEntry(hostName);

      foreach (var address in hostEntry.AddressList)
      {
        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
          return address.ToString();
        }
      }

      return "IPv4 address not found.";
    }
    catch (Exception ex)
    {
      return "Error: " + ex.Message;
    }
  }
}