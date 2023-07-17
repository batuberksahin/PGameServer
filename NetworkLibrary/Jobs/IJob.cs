using System.Threading.Tasks;

namespace NetworkLibrary.Jobs;

public interface IJob
{
  Task RunAsync();
}