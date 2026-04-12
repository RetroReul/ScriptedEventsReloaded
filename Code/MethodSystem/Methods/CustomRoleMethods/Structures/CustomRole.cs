using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using SER.Code.Helpers;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;

public class CustomRole
{
    public static readonly Dictionary<string, CustomRole> RegisteredRoles = [];
    public static readonly Dictionary<Player, CustomRole> AssignedRoles = [];
    public static readonly Dictionary<Player, CustomRole> LastRoles = [];
    
    public required string DisplayName;
    public required RoleTypeId RoleType;
    public CustomRoleSpawnSystem? SpawnInfo;
    public Action<Value[]>? SpawnAction;
    public Action<Value[]>? RemoveAction;
    public bool RemoveRoleOnDeath = true;

    public static void ResetAll()
    {
        AssignedRoles.Clear();
        LastRoles.Clear();
        RegisteredRoles.Clear();
    }
    
    public void AssignPlayer(Player plr)
    {
        if (AssignedRoles.TryGetValue(plr, out var previousRole))
        {
            LastRoles[plr] = previousRole;
            AssignedRoles.Remove(plr);
        }
        
        AssignedRoles.Add(plr, this);
        
        plr.SetRole(RoleType);
        
        SpawnAction?.Invoke([
            new ReferenceValue(this),
            new PlayerValue(plr)
        ]);
        
        plr.InfoArea |= PlayerInfoArea.CustomInfo;
        plr.InfoArea &= ~PlayerInfoArea.Role;
        plr.InfoArea &= ~PlayerInfoArea.Nickname;
        plr.InfoArea &= ~PlayerInfoArea.UnitName;

        plr.CustomInfo = $"{plr.DisplayName}\n{DisplayName}";
    }

    public void RemovePlayer(Player plr)
    {
        plr.CustomInfo = "";
        
        AssignedRoles.Remove(plr);
        RemoveAction?.Invoke([
            new ReferenceValue(this),
            new PlayerValue(plr)
        ]);
    }
    
    public static void Register()
    {
        PlayerEvents.Death += OnDeath;
        ServerEvents.RoundStarted += OnRoundStarted;
    }

    private static void OnRoundStarted()
    {
        foreach (var role in RegisteredRoles.Values)
        {
            switch (role.SpawnInfo)
            {
                case null:
                    continue;
                case ProceduralSpawn procedural:
                {
                    Log.Signal(1);
                    var pool = Player.List.Where(p => p.Role == procedural.RoleToReplace).ToArray();
                    if (pool.Length < procedural.StartSpawningWhen)
                    {
                        Log.Signal(2);
                        continue;
                    }
            
                    var playersToAssign = pool
                        .Where(p => !AssignedRoles.ContainsKey(p))
                        .Where(_ => procedural.SpawnChance > new Random().NextDouble())
                        .ToList();
                
                    playersToAssign.ShuffleList();
                    if (procedural.MaxAmountToSpawn is { } maxAmountToSpawn)
                    {
                        playersToAssign = playersToAssign.Take(maxAmountToSpawn).ToList();
                    }
            
                    Log.Signal(3);
                    playersToAssign.ForEach(p => role.AssignPlayer(p));
                    break;
                }
                case BracketSpawn bracketSpawn:
                {
                    Log.Signal(4);
                    var pool = Player.List.Where(p => p.Role == bracketSpawn.RoleToReplace).ToArray();
                
                    foreach (var bracket in bracketSpawn.SpawnBrackets)
                    {
                        if (bracket.LowerBound > pool.Length || pool.Length > bracket.UpperBound)
                        {
                            Log.Signal(5);
                            continue;
                        }

                        var availablePlayers = pool
                            .Where(p => !AssignedRoles.ContainsKey(p))
                            .ToList();

                        for (int i = 0; i < bracket.AmountToSpawn; i++)
                        {
                            if (availablePlayers.Count <= 0) return;
                        
                            var plr = availablePlayers.PullRandomItem();
                            Log.Signal(6);
                            role.AssignPlayer(plr);
                        }
                    }

                    break;
                }
            }
        }
        
        Log.Signal(7);
    }

    public static void OnDeath(PlayerDeathEventArgs ev)
    {
        if (ev.Player is not {} plr) return;
        if (!AssignedRoles.TryGetValue(plr, out var role)) return;
        if (!role.RemoveRoleOnDeath) return;
        
        role.RemovePlayer(plr);
    }
}