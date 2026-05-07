using Interactables.Interobjects.DoorUtils;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DoorMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Door_LockMethod : SynchronousMethod
{
    public override string Description => "Locks doors.";

    public override Argument[] ExpectedArguments { get; } = 
    [       
        new DoorsArgument("doors"),
        new EnumArgument<DoorLockReason>("lock")
        {
            DefaultValue = new(DoorLockReason.AdminCommand, null)
        }
    ];
    
    public override void Execute()
    {
        var doors = Args.GetDoors("doors");
        var lockType = Args.GetEnum<DoorLockReason>("lock");
        
        foreach (var door in doors)
        {
            door.Lock(lockType, true);
        }
    }
}