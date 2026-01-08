using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.ProjectileMethods;

[UsedImplicitly]
public class SpawnProjectileMethod : SynchronousMethod
{
    public override string Description => "Spawns a live projectile at a given position.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new EnumArgument<ProjectileType>("projectile type"),
        new FloatArgument("position x"),
        new FloatArgument("position y"),
        new FloatArgument("position z"),
        new PlayerArgument("owner")
        {
            DefaultValue = new(null, "server")
        },
        new FloatArgument("time until detonation")
        {
            DefaultValue = new(-1f, "default")
        }
    ];

    public override void Execute()
    {
        var projectileType = Args.GetEnum<ProjectileType>("projectile type") switch
        {
            ProjectileType.Scp018 => ItemType.SCP018,
            ProjectileType.Grenade => ItemType.GrenadeHE,
            ProjectileType.Flashbang => ItemType.GrenadeFlash,
            ProjectileType.Scp2176 => ItemType.SCP2176,
            _ => throw new ArgumentOutOfRangeException()
        };
        var positionX = Args.GetFloat("position x");
        var positionY = Args.GetFloat("position y");
        var positionZ = Args.GetFloat("position z");
        var owner = Args.GetPlayer("owner");
        var timeUntilDetonation = Args.GetFloat("time until detonation");

        TimedGrenadeProjectile.SpawnActive(
            new(
                positionX,
                positionY,
                positionZ
            ),
            projectileType,
            owner,
            timeUntilDetonation
        );
    }

    public enum ProjectileType
    {
        Scp018,
        Grenade,
        Flashbang,
        Scp2176
    }
}