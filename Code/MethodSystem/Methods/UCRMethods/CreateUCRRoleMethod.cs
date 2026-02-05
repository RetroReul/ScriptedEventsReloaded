using JetBrains.Annotations;
using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Structures;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;

namespace SER.Code.MethodSystem.Methods.UCRMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class CreateUCRRoleMethod : SynchronousMethod, ICanError, IDependOnFramework
{
    public override string Description => "Creates a custom UCR role.";

    public IDependOnFramework.Type DependsOn => IDependOnFramework.Type.Ucr;

    public string[] ErrorReasons { get; } =
    [
        "Role with the same id already exists."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new IntArgument("id", 0),
        new TextArgument("name"),
        new EnumArgument<RoleTypeId>("role type")
    ];

    public override void Execute()
    {
        var role = new CustomRole
        {
            Id = Args.GetInt("id"),
            Name = Args.GetText("name"),
            Role = Args.GetEnum<RoleTypeId>("role type")
        };

        switch (CustomRole.Register(role))
        {
            case LoadStatusType.SameId: 
                throw new ScriptRuntimeError(this, ErrorReasons[0]);
            
            case LoadStatusType.Success:
                return;
            
            case LoadStatusType.ValidatorError:
                throw new AndrzejFuckedUpException("Validator error of UCR role.");
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}