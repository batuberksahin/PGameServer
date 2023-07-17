namespace NetworkLibrary.Jobs;

public class JobScheduler
{
  private readonly TimeSpan                _interval;
  private readonly List<IJob>              _jobs;
  private          CancellationTokenSource _cancellationTokenSource;

  public JobScheduler(TimeSpan interval)
  {
    _interval = interval;
    _jobs     = new List<IJob>();
  }

  public void RegisterTask(IJob job)
  {
    _jobs.Add(job);
  }

  public void Start()
  {
    _cancellationTokenSource = new CancellationTokenSource();

    Task.Run(() => RunJobsPeriodically(_cancellationTokenSource.Token));
  }

  public void Stop()
  {
    _cancellationTokenSource?.Cancel();
  }

  private async Task RunJobsPeriodically(CancellationToken cancellationToken)
  {
    while (!cancellationToken.IsCancellationRequested)
    {
      await Task.Delay(_interval, cancellationToken);

      foreach (var job in _jobs) await job.RunAsync();
    }
  }
}