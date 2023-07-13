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

        string behaviourName = behaviourParts[0];
        string payload = behaviourParts[1];

        if (_behaviours.TryGetValue(behaviourName, out IBehaviour behaviour))
        {
            await behaviour.ProcessRequestAsync(client, payload);
        }
    }
}