using JetBrains.Annotations;
using MEC;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.VariableSystem.Bases;

namespace SER.MethodSystem.Methods.ScriptMethods;

[UsedImplicitly]
public class RunScriptAndWaitMethod : YieldingMethod
{
    public override string Description => "Runs a script and waits until the ran script has finished.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new CreatedScriptArgument("script to create"),
        new VariableArgument("variables to pass")
        {
            ConsumesRemainingValues = true,
            DefaultValue = new(new List<Variable>(), "none"),
            Description = "Passes an exact copy of the provided variables to the script."
        }
    ];
    
    public override IEnumerator<float> Execute()
    {
        var script = Args.GetCreatedScript("script to create");
        var variables = Args.GetRemainingArguments<Variable, VariableArgument>("variables to pass");
        
        script.AddVariables(variables);
        script.Run();
        
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        while (script.IsRunning)
        {
            yield return Timing.WaitForOneFrame;
        }
    }
}