using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using SER.Plugin.Commands.Interfaces;
using SER.ScriptSystem;

namespace SER.Plugin.Commands;

[CommandHandler(typeof(GameConsoleCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class RunningScriptsCommand : ICommand, IUsePermissions
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var player = Player.Get(sender);
        if (player is not null && player.HasPermissions(Permission))
        {
            response = "You do not have permission see running scripts.";
            return false;
        }
        
        response = $"There are {Script.RunningScripts.Count} running scripts" 
                   + Script.RunningScripts
                       .Select(s => s.Name)
                       .Aggregate(string.Empty, (a, b) => $"{a}\n> {b}");
        return true;
    }

    public string Command => "serrunning";
    public string[] Aliases => [];
    public string Description => "Returns a list of names of all running scripts.";
    public string Permission => "ser.run";
}