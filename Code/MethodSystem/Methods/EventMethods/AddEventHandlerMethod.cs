using LabApi.Events.Arguments.Interfaces;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ScriptSystem.Structures;
using EventHandler = SER.Code.EventSystem.EventHandler;

namespace SER.Code.MethodSystem.Methods.EventMethods;

[UsedImplicitly]
public class AddEventHandlerMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Adds an event handler to the provided event.";

    public string AdditionalDescription =>
        "Runs the function when the event happens. Prefer the '!-- OnEvent' flag when you can. " +
        "Event variables such as @evPlayer are added automatically, so do not list them as function arguments. " +
        "Calling AddEventHandler again for the same function replaces its previous event handler.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new EventArgument("event name"),
        new CallbackArgument("callback"),
    ];

    public override void Execute()
    {
        var eventName = Args.GetEvent("event name");
        var callback = Args.GetCallback("callback");
        var handlerId = $"function '{callback.Name}' in script '{Script.Name}'";
        
        var result = EventHandler.AddEventHandler(
            eventName, 
            (args, vars) => callback.Action([], scr =>
            {
                scr.AddLocalVariables(vars);
                if (args is not ICancellableEvent cancellable)
                {
                    scr.Run(RunReason.FunctionCallback);
                    return;
                }

                scr.RunForEvent(
                    RunReason.FunctionCallback,
                    eventAllowedChanged: isAllowed => cancellable.IsAllowed = isAllowed);
            }),
            handlerId
        );

        if (result.HasErrored(out var error))
        {
            throw new ScriptRuntimeError(this, error);
        }
    }
}
