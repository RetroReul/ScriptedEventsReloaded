using JetBrains.Annotations;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.HealthMethods;

[UsedImplicitly]
public class KillMethod : SynchronousMethod
{
    public override string Description => "Kills players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new TextArgument("reason")
        {
            DefaultValue = new(string.Empty, "empty"),
        }
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        
        foreach (var player in players)
        {
            player.Kill();
        }
    }
}