using PlayerRoles;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;

public class ProceduralSpawn : CustomRoleSpawnSystem
{
    public required RoleTypeId RoleToReplace;
    public required float SpawnChance;
    public int? MaxAmountToSpawn;
    public int? StartSpawningWhen;
}