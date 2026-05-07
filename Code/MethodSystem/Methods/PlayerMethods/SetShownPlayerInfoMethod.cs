using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class SetShownPlayerInfoMethod : SynchronousMethod
{
    public override string Description => "Sets what information about the player is shown.";

    public override Argument[] ExpectedArguments { get; } = 
    [
        new PlayersArgument("players"),
        new EnumArgument<PlayerInfoArea>("info to show")
        {
            DefaultValue = new((PlayerInfoArea)0, "nothing", true)
        }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var info = Args.GetEnum<PlayerInfoArea>("info to show");
        
        foreach (var player in players) player.InfoArea = info;
    }
}
