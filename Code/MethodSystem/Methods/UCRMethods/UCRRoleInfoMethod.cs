using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;
using UncomplicatedCustomRoles.API.Interfaces;

namespace SER.Code.MethodSystem.Methods.UCRMethods;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class GetUCRRoleInfoMethod : LiteralValueReturningMethod, IReferenceResolvingMethod, IDependOnFramework
{
    public Type ResolvesReference => typeof(ICustomRole);

    public IDependOnFramework.Type DependsOn => IDependOnFramework.Type.Ucr;

    public override TypeOfValue LiteralReturnTypes => new TypesOfValue([
        typeof(NumberValue), 
        typeof(TextValue)
    ]);

    public override string Description => "Returns information about a custom role.";
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<ICustomRole>("custom role reference"),
        new OptionsArgument("property",
            "id",
            "name"
        )
    ];

    public override void Execute()
    {
        var role = Args.GetReference<ICustomRole>("custom role reference");
        ReturnValue = Args.GetOption("property") switch
        {
            "id" => new NumberValue(role.Id),
            "name" => new StaticTextValue(role.Name),
            _ => throw new AndrzejFuckedUpException()
        };
    }
}