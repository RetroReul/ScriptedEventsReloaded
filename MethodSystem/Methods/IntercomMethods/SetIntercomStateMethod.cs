using JetBrains.Annotations;
using SER.MethodSystem.BaseMethods;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using PlayerRoles.Voice;

namespace SER.MethodSystem.Methods.IntercomMethods;

[UsedImplicitly]
public class SetIntercomStateMethod : SynchronousMethod
{
    public override string Description => "Sets the state of usage of the intercom.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new EnumArgument<IntercomState>("state")
    ];

    public override void Execute()
    {
        Intercom.State = Args.GetEnum<IntercomState>("state");
    }
}