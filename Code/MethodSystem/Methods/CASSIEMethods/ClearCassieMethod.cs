using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.CASSIEMethods;

[UsedImplicitly]
public class ClearCassieMethod : SynchronousMethod
{
    public override string Description => "Clears all CASSIE announcements, active or queued.";
    
    public override Argument[] ExpectedArguments { get; } = [];
    
    public override void Execute()
    {
        LabApi.Features.Wrappers.Cassie.Clear();
    }
}