using JetBrains.Annotations;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;
using SER.VariableSystem.Variables;

namespace SER.MethodSystem.Methods.CollectionVariableMethods;

[UsedImplicitly]
public class CollectionRemoveMethod : SynchronousMethod
{
    public override string Description => "Removes a matching value from a collection variable";

    public override Argument[] ExpectedArguments { get; } =
    [
        new CollectionVariableArgument("collection variable"),
        new AnyValueArgument("value to remove"),
        new IntArgument("amount of matches to remove", -1)
        {
            Description = "Will delete every match if -1.",
            DefaultValue = new(-1, null)
        }
    ];

    public override void Execute()
    {
        var collectionVar = Args.GetCollectionVariable("collection variable");
        var amountToRemove = Args.GetInt("amount of matches to remove");
        var value = Args.GetAnyValue("value to remove");
        
        Script.AddVariable(
            new CollectionVariable(
                collectionVar.Name,
                CollectionValue.Remove(collectionVar.Value, value, amountToRemove)
            )
        );
    }
}