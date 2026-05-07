using LabApi.Features.Wrappers;
using Mirror;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.PickupMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Pickup_DestroyMethod : SynchronousMethod, ICanError
{
    public override string Description => "Destroys a pickup / grenade from the map.";

    public string[] ErrorReasons =>
    [
        "The pickup/projectile hasn't been spawned yet or doesn't exist anymore."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Pickup>("pickup/projectile reference"),
    ];

    public override void Execute()
    {
        var obj = Args.GetReference<Pickup>("pickup/projectile reference");
        
        if (!NetworkServer.spawned.ContainsValue(obj.NetworkIdentity))
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        
        obj.Destroy();
    }
}