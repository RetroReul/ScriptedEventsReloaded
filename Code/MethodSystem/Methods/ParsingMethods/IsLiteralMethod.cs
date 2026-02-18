using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ParsingMethods;

[UsedImplicitly]
public class IsLiteralMethod : ReturningMethod<BoolValue>
{
    public override string Description => $"Returns true if given value is a {nameof(LiteralValue)}";

    public override Argument[] ExpectedArguments { get; } =
    [
        new AnyValueArgument("value to check")
    ];
    
    public override void Execute()
    {
        ReturnValue = Args.GetAnyValue("value to check") is LiteralValue;
    }
}