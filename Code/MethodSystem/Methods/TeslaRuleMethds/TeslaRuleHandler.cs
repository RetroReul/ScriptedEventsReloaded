using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using PlayerRoles;

namespace SER.Code.MethodSystem.Methods.TeslaRuleMethds;

public class TeslaRuleHandler : CustomEventsHandler
{
    public static HashSet<RoleTypeId> IgnoredRoles = [];
    public static HashSet<Team> IgnoredTeams = [];
    public static HashSet<int> IgnoredPlayerIds = [];

    public static void ResetAll()
    {
        IgnoredRoles.Clear();
        IgnoredTeams.Clear();
        IgnoredPlayerIds.Clear();
    }

    public override void OnPlayerTriggeringTesla(PlayerTriggeringTeslaEventArgs ev)
    {
        if (ev.Player is not {} plr) return;
        if (
            IgnoredRoles.Contains(plr.Role)
            || IgnoredTeams.Contains(plr.Team)
            || IgnoredPlayerIds.Contains(plr.PlayerId))
        {
            ev.IsAllowed = false;
        }
    }
}