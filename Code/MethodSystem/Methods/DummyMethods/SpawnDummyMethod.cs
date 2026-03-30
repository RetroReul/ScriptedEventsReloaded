
using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DummyMethods;

[UsedImplicitly]
public class SpawnDummyMethod : SynchronousMethod
{
    public override string Description => "Spawns a dummy";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("name"),
        new EnumArgument<RoleTypeId>("role type")
    ];
    
    public override void Execute()
    {
        var name = Args.GetText("name");
        var roleType = Args.GetEnum<RoleTypeId>("role type");

        var dummy = Player.Get(DummyUtils.SpawnDummy(name));
        dummy.SetRole(roleType);
    }
}