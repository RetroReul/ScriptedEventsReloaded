using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.PickupMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Pickup_SpawnInRoomMethod : SynchronousMethod, ICanError
{
    public override string Description => "Spawns an item pickup / grenade in a room.";

    public string[] ErrorReasons => Pickup_SpawnOnPositionMethod.Singleton.ErrorReasons;

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Pickup>("pickup/projectile reference"),
        new RoomArgument("room to spawn pickup in"),
        new FloatArgument("relative x")
        {
            DefaultValue = new(0, null)
        },
        new FloatArgument("relative y")
        {
            DefaultValue = new(0, null)
        },
        new FloatArgument("relative z")
        {
            DefaultValue = new(0, null)
        },
    ];

    public override void Execute()
    {
        var obj = Args.GetReference<Pickup>("pickup/projectile reference");
        var room = Args.GetRoom("room to spawn pickup in");
        var pos = room.Transform.TransformPoint(new(
            Args.GetFloat("relative x"),
            Args.GetFloat("relative y"),
            Args.GetFloat("relative z")));

        Pickup_SpawnOnPositionMethod.SpawnPickup(obj, pos, this);
    }
}