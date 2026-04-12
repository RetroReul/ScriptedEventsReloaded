using PlayerRoles;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;

public class BracketSpawn : CustomRoleSpawnSystem
{
    public required RoleTypeId RoleToReplace;
    public required SpawnBracket[] SpawnBrackets;
}