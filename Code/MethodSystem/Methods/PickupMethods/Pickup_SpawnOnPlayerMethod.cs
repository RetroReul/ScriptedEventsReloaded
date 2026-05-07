using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.PickupMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Pickup_SpawnOnPlayerMethod : SynchronousMethod, ICanError
{
    public override string Description => "Spawns an item pickup / grenade on a player.";
    
    public string[] ErrorReasons => Pickup_SpawnOnPositionMethod.Singleton.ErrorReasons;

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Pickup>("pickup/projectile reference"),
        new PlayerArgument("player to spawn pickup on"),
    ];

    public override void Execute()
    {
        var obj = Args.GetReference<Pickup>("pickup/projectile reference");
        var plr = Args.GetPlayer("player to spawn pickup on");

        Pickup_SpawnOnPositionMethod.SpawnPickup(obj, plr.Position, this);
    }
}