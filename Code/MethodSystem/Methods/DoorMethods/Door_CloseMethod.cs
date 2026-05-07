using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DoorMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Door_CloseMethod : SynchronousMethod
{
    public override string Description => "Closes doors.";

    public override Argument[] ExpectedArguments { get; } =
    [       
        new DoorsArgument("doors")
    ];
    
    public override void Execute()
    {
        var doors = Args.GetDoors("doors");

        foreach (var door in doors)
        {
            door.IsOpened = false;
        }
    }
}