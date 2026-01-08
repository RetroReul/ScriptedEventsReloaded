using JetBrains.Annotations;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.CASSIEMethods;

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