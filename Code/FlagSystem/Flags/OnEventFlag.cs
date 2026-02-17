using JetBrains.Annotations;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using EventHandler = SER.Code.EventSystem.EventHandler;

namespace SER.Code.FlagSystem.Flags;

[UsedImplicitly]
public class OnEventFlag : Flag
{
    public override string Description =>
        "Binds a script to an in-game event. When the event happens, the script will execute. " +
        "Events can sometimes also carry information of their own, ";

    public override Argument? InlineArgument => new(
        "eventName",
        "The name of the event to bind to.",
        inlineArgs =>
        {
            switch (inlineArgs.Length)
            {
                case < 1:
                    return "Event name is missing";
                case > 1:
                    return "Too many arguments, only event name is allowed";
            }

            if (EventHandler.ConnectEvent(inlineArgs.First(), ScriptName).HasErrored(out var error))
            {
                return error;
            }

            return true;
        },
        true
    );

    public override Result OnScriptRunning(Script scr)
    {
        if (scr.Context == RunContext.Event)
        {
            return true;
        }
        
        return $"A script using '{Name}' flag cannot be ran by any other mean than an event.";
    }

    public override Argument[] Arguments => [];

    public override void Unbind()
    {
        // done by event handler
    }
}