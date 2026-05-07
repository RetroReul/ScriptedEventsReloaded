using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class SetDisplayNameMethod : SynchronousMethod
{
    public override string Description => "Sets display name for specified players";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new TextArgument("display name")
        {
            DefaultValue = new("", "removes display name", true),
        }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var displayName = Args.GetText("display name");

        foreach (var p in players) p.DisplayName = displayName;
    }
}
