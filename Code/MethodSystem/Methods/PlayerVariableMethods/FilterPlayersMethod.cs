using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.PropertySystem;

namespace SER.Code.MethodSystem.Methods.PlayerVariableMethods;

[UsedImplicitly]
public class FilterPlayersMethod : ReturningMethod<PlayerValue>
{
    public override string Description => "Returns players which match the value for a given property.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players to filter"),
        new EnumArgument<PlayerValue.PlayerProperty>("player property"),
        new AnyValueArgument("desired value")
    ];
    
    public override void Execute()
    {
        var playersToFilter = Args.GetPlayers("players to filter");
        var playerProperty = Args.GetEnum<PlayerValue.PlayerProperty>("player property");
        var desiredValue = Args.GetAnyValue("desired value");
        var handler = ((IValueWithProperties.PropInfo<Player>)PlayerValue.PropertyInfoMap[playerProperty]).Func;

        ReturnValue = new(playersToFilter.Where(p => handler(p) == desiredValue));
    }
}