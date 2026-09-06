using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

[UsedImplicitly]
public class GiveAmmoMethod : SynchronousMethod, IEssential
{
    public override string Description => "Gives ammo to players.";

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
            var amountToGive = Math.Min(amount, ushort.MaxValue - player.GetAmmo(ammoType));
            if (amountToGive > 0)
            {
                player.AddAmmo(ammoType, (ushort)amountToGive);
            }
        }
    }
}
