using NetworkLibrary.Behaviours;

namespace NetworkLibrary;

public interface ITcpServer
{
  Task StartAsync();
  void StopImmediately();

  void RegisterBehaviour(string behaviorName, IBehaviour behavior);
}