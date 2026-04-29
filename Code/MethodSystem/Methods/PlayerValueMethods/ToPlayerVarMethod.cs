using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerValueMethods;

[UsedImplicitly]
public class ToPlayerVarMethod : ReturningMethod<PlayerValue>
{
    public override string Description => "Parses a player argument into a player variable";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("input")
    ];

    public override void Execute()
    {
        ReturnValue = new PlayerValue(Args.GetPlayers("input"));
    }
}