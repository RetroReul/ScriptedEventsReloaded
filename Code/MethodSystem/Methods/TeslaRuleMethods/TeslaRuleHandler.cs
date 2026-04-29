using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;

namespace SER.Code.MethodSystem.Methods.TeslaRuleMethods;

public class TeslaRuleHandler : CustomEventsHandler
{
    public struct TeslaIgnoreRule
    {
        public required string? Id;
        public required Func<Player[]> Players;
    }
    
    public static readonly List<TeslaIgnoreRule> Rules = [];

    public static void ResetAll()
    {
        Rules.Clear();
    }

    public override void OnPlayerIdlingTesla(PlayerIdlingTeslaEventArgs ev)
    {
        if (ev.Player is not {} plr) return;

        foreach (var rule in Rules)
        {
            if (rule.Players().Contains(plr))
            {
                ev.IsAllowed = false;
                return;
            }
        }
    }

    public override void OnPlayerTriggeringTesla(PlayerTriggeringTeslaEventArgs ev)
    {
        if (ev.Player is not {} plr) return;

        foreach (var rule in Rules)
        {
            if (rule.Players().Contains(plr))
            {
                ev.IsAllowed = false;
                return;
            }
        }
    }
}