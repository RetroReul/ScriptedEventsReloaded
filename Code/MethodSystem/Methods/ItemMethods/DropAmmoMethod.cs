using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

[UsedImplicitly]
public class DropAmmoMethod : SynchronousMethod
{
    public override string Description => "Drops ammo from players' inventories.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        AmmoMethodHelper.CreateArgument(),
        new IntArgument("amount", 1, ushort.MaxValue)
        {
            DefaultValue = new(1, null)
        }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var ammoType = AmmoMethodHelper.Parse(Args.GetOption(AmmoMethodHelper.ArgumentName));
        var amount = Args.GetInt("amount");

        foreach (var player in players)
        {
            var amountToDrop = Math.Min(amount, player.GetAmmo(ammoType));
            if (amountToDrop > 0)
            {
                player.DropAmmo(ammoType, (ushort)amountToDrop, false);
            }
        }
    }
}
