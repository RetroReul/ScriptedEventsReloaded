using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

[UsedImplicitly]
public class GetAmmoMethod : ReturningMethod<NumberValue>
{
    public override string Description => "Returns how much ammo a player has.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player"),
        AmmoMethodHelper.CreateArgument()
    ];

    public override void Execute()
    {
        var player = Args.GetPlayer("player");
        var ammoType = AmmoMethodHelper.Parse(Args.GetOption(AmmoMethodHelper.ArgumentName));

        ReturnValue = player.GetAmmo(ammoType);
    }
}
