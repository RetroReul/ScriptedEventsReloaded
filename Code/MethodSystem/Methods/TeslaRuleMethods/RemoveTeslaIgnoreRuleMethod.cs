using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.TeslaRuleMethods;

[UsedImplicitly]
public class RemoveTeslaIgnoreRuleMethod : SynchronousMethod
{
    public override string Description => "Resets the list of players ignored by a tesla.";

    public override Argument[] ExpectedArguments { get; } = 
    [
        new TextArgument("id to remove")
        {
            DefaultValue = new(null, "Removes all tesla ignore rules")
        }
    ];

    public override void Execute()
    {
        if (Args.GetText("id to remove") is { } id)
        {
            TeslaRuleHandler.Rules.RemoveAll(x => x.Id == id);
        }
        else
        {
            TeslaRuleHandler.Rules.Clear();
        }
    }
}
