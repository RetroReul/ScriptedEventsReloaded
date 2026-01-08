using JetBrains.Annotations;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.ScriptMethods;

[UsedImplicitly]
public class StopScriptMethod : SynchronousMethod
{
    public override string Description => "Stops a given script.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new RunningScriptArgument("running script")
    ];
    
    public override void Execute()
    {
        Args.GetRunningScript("running script").Stop();
    }
}