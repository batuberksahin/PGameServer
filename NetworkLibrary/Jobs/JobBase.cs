using System.Net.Sockets;

namespace NetworkLibrary.Jobs;

public abstract class JobBase : IJob
{
    protected JobBase()
    {
    }

    public abstract Task RunAsync();
}