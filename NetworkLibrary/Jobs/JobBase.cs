using System.Net.Sockets;
using System.Text;

namespace NetworkLibrary.Jobs;

public abstract class JobBase : IJob
{
  protected JobBase()
  {
  }

  public abstract Task StartAsync();
  public abstract Task RunAsync();
}