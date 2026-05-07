using Interactables.Interobjects.DoorUtils;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DoorMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Door_SetPermissionMethod : SynchronousMethod
{
    public override string Description => "Sets door permissions.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new DoorsArgument("doors"),
        new EnumArgument<DoorPermissionFlags>("permissions")
    ];
    
    public override void Execute()
    {
        var doors = Args.GetDoors("doors");
        var permissions = Args.GetEnum<DoorPermissionFlags>("permissions");

        foreach (var door in doors) door.Permissions = permissions;
    }
}