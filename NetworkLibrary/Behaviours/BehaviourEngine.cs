using System.Net.Sockets;

namespace NetworkLibrary.Behaviours;

public class BehaviourEngine
{
  private readonly Dictionary<string, IBehaviour> _behaviours;

  public BehaviourEngine()
  {
    _behaviours = new Dictionary<string, IBehaviour>();
  }

  public void RegisterBehaviour(string behaviourName, IBehaviour behaviour)
  {
    _behaviours[behaviourName] = behaviour;

    Console.WriteLine($"\"{behaviourName}\" behaviour registered.");
  }

  public async Task ParseRequestAsync(TcpClient client, string behaviourString)
  {
    string[] behaviourParts = behaviourString.Split(new[] { ':' }, 2);
    if (behaviourParts.Length < 1)
      return;

    var behaviourName = behaviourParts[0];
    var payload       = behaviourParts[1];

    if (_behaviours.TryGetValue(behaviourName, out var behaviour)) await behaviour.ProcessRequestAsync(client, payload);
  }
}