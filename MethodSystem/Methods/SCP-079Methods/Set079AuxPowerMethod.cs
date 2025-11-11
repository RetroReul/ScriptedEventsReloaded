using LabApi.Features.Wrappers;
using PlayerRoles.PlayableScps.Scp079;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.SCP079Methods;
public class Set079AuxPowerMethod : SynchronousMethod
{
    public override string Description => "Sets the Auxiliary Power of the given player(s) if they are SCP-079";

    public override Argument[] ExpectedArguments =>
    [
        new PlayersArgument("players"),
        new IntArgument("power", 0)
    ];

    public override void Execute()
    {
        var plrs = Args.GetPlayers("players");
        var value = Args.GetInt("power");
        foreach(Player p in plrs)
        {
            if(p.RoleBase is Scp079Role scp)
            {
                if(scp.SubroutineModule.TryGetSubroutine<Scp079AuxManager>(out Scp079AuxManager aux))
                {
                    aux.CurrentAux = value;
                }
            }
        }
    }
}
