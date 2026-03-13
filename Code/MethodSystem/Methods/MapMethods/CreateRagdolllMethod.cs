using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerStatsSystem;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.MapMethods;

[UsedImplicitly]
public class SpawnRagdollMethod : SynchronousMethod, ICanError
{
    public override string Description => "Spawns a ragdoll.";

    public override Argument[] ExpectedArguments =>
    [
        new EnumArgument<RoleTypeId>("role"),
        new TextArgument("name"),
        new ReferenceArgument<Vector3>("position"),
        new ReferenceArgument<Vector3?>("scale")
        { DefaultValue = new(null, "default size") },
        new ReferenceArgument<Quaternion>("rotation")
        { DefaultValue = new(Quaternion.identity, "default rotation") },
        new AnyValueArgument("damage handler")
        {
            DefaultValue = new(new CustomReasonDamageHandler(""), "damage reason will be blank"),
            Description = $"Accepts a {nameof(TextValue)} or a {nameof(DamageHandlerBase)} reference."
        },
    ];
    
    public override void Execute()
    {
        var role = Args.GetEnum<RoleTypeId>("role");
        var name = Args.GetText("name");
        var position = Args.GetReference<Vector3>("position");
        var scale = Args.GetReference<Vector3?>("scale");
        var rotation = Args.GetReference<Quaternion>("rotation");
        var value = Args.GetAnyValue("damage handler");
        
        DamageHandlerBase? damageHandler = null;
        
        switch (value)
        {
            case ReferenceValue referenceValue:
            {
                if (referenceValue.Value is DamageHandlerBase handler)
                    damageHandler = handler;
                else
                    throw new ScriptRuntimeError(this, ErrorReasons[1]);
                break;
            }
            case TextValue textValue:
                damageHandler = new CustomReasonDamageHandler(textValue.StringRep);
                break;
        }
        
        if (damageHandler is null)
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        
        Ragdoll.SpawnRagdoll(role, position, rotation, damageHandler, name, scale);
    }

    public string[] ErrorReasons =>
    [
        $"Damage handler value must be a {nameof(DamageHandlerBase)} reference or a {nameof(TextValue)}.",
        $"The provided reference value was not a {nameof(DamageHandlerBase)} reference."
    ];
}