using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods;

[UsedImplicitly]
public class RegisterCustomRoleMethod : SynchronousMethod, ICanError
{
    public override string Description => "Registers a custom role.";

    public string[] ErrorReasons =>
    [
        "Provided custom role id is already registered.",
        "Display name uses an invalid format."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("id", false)
        {
            Description = "This will be used to identify the role."
        },
        new TextArgument("display name"),
        new EnumArgument<RoleTypeId>("role type"),
        new ReferenceArgument<CustomRoleSpawnSystem>("spawn system")
        {
            DefaultValue = new(null, "the role will not be spawned automatically")
        },
        new BoolArgument("remove role on death")
        {
            DefaultValue = new(true, null),
        },
        new CallbackArgument("on spawning", 
            (typeof(ReferenceValue<CustomRole>), "role"),
            (typeof(PlayerValue), "player"))
        {
            Description = "This will be called when a player is being spawned with this role.",
            DefaultValue = new(null, "no spawning callback")
        },
        new CallbackArgument("on removing", 
            (typeof(ReferenceValue<CustomRole>), "role"),
            (typeof(PlayerValue), "player"))
        {
            Description = "This will be called when this role is being taken away from a player.",
            DefaultValue = new(null, "no removing callback")
        }
    ];

    public override void Execute()
    {
        var id = Args.GetText("id");
        if (CustomRole.RegisteredRoles.ContainsKey(id))
        {
            throw new ScriptRuntimeError(this, $"Provided custom role id {id} is already registered.");
        }
        
        var displayName = Args.GetText("display name");
        if (!Player.ValidateCustomInfo(displayName, out var reason))
        {
            throw new ScriptRuntimeError(this, $"Display name uses an invalid format: {reason}");
        }
        
        CustomRole.RegisteredRoles.Add(
            id,
            new()
            {
                DisplayName = displayName,
                RoleType = Args.GetEnum<RoleTypeId>("role type"),
                RemoveRoleOnDeath = Args.GetBool("remove role on death"),
                SpawnInfo = Args.GetReference<CustomRoleSpawnSystem>("spawn system"),
                SpawnAction = Args.GetCallback("on spawning"),
                RemoveAction = Args.GetCallback("on removing")
            }
        );
    }
}