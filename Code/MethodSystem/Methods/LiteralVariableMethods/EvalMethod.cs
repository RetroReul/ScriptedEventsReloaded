using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.LiteralVariableMethods;

[UsedImplicitly]
public class EvalMethod : ReturningMethod
{
    public override string Description => 
        "Evaluates the provided expression and returns the result. Used for math operations.";
    public override Type[]? ReturnTypes => null;

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("value")
    ];

    public override void Execute()
    {
        var value = Args.GetText("value");
        if (NumericExpressionReslover.EvalString(value, Script).HasErrored(out var error, out var result))
        {
            throw new ScriptRuntimeError(error);
        }
        
        ReturnValue = Value.Parse(result);
    }
}