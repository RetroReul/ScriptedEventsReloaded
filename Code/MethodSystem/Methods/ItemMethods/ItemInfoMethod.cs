using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.TokenSystem.Tokens.ExpressionTokens;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

[UsedImplicitly]
public class ItemInfoMethod : ReturningMethod, IReferenceResolvingMethod
{
    public override string Description => IReferenceResolvingMethod.Desc.Get(this);
    
    public Type ResolvesReference => typeof(Item);

    public override TypeOfValue Returns => ReferenceVariableExpressionToken.GetTypesOfValue(ResolvesReference);

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Item>("reference"),
        new OptionsArgument("property", 
            Option.Enum<ItemType>("type"),
            Option.Enum<ItemCategory>("category"),
            "owner",
            "isEquipped"
        )
    ];

    public override void Execute()
    {
        var item = Args.GetReference<Item>("reference");
        ReturnValue = ReferenceVariableExpressionToken
            .PropertyInfoMap
            [typeof(Item)]
            [Args.GetOption("property")]
            .Handler(item);
    }
}