using JetBrains.Annotations;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using Utils;

namespace SER.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class ExplodeMethod : SynchronousMethod
{
    public override string Description => "Explodes players.";
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players")
    ];

    public override void Execute()
    {
        Args.GetPlayers("players").ForEach(player => 
            ExplosionUtils.ServerExplode(player.ReferenceHub, ExplosionType.Grenade));
    }
}