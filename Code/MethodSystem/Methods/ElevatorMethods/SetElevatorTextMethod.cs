using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.ElevatorMethods;

[UsedImplicitly]
public class SetElevatorTextMethod : SynchronousMethod
{
    public override string Description => "Changes the text on the elevator panels between LCZ and HCZ.";

    public override Argument[] ExpectedArguments =>
    [
        new TextArgument("new text")
        {
            DefaultValue = new(string.Empty, "Resets the text to it's original value."),
            Description = "An empty value will reset the text."
        }
    ];
    
    public override void Execute()
    {
        Decontamination.ElevatorsText = Args.GetText("text");
    }
}