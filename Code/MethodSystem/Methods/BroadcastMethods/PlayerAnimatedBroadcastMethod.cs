using Cassie;
using Exiled.API.Extensions;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Structures;
using Player = Exiled.API.Features.Player;

namespace SER.Code.MethodSystem.Methods.BroadcastMethods;

[UsedImplicitly]
public class PlayerAnimatedBroadcastMethod : SynchronousMethod, IAdditionalDescription, IDependOnFramework
{
    public override string Description => "Sends an animated broadcast to specified players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new DurationArgument("duration"),
        new TextArgument("content"),
        new IntArgument("line break length")
        {
            Description = "How many characters are needed to make a new line",
            DefaultValue = new(60, null)
        }
    ];

    public override void Execute()
    {
        var content = Args.GetText("content");
        var duration = Args.GetDuration("duration").TotalSeconds;

        foreach (var labPlr in Args.GetPlayers("players"))
        {
            var plr = Player.Get(labPlr);
            plr.Connection.Send(new CassieTtsPayload(string.Empty, string.Empty, false));
            plr.MessageTranslated(
                $"$SLEEP_{duration-1} .",
                null!,
                AnimatedBroadcastMethod.Helper.FormatToCassieCentralScreenSubtitles(content, Args.GetInt("line break length"))
            );
        }
    }

    public string AdditionalDescription =>
        "Uses CASSIE to make an animated broadcast - if there is CASSIE playing, it will be stopped. " +
        "Keep custom formatting to a minimum, this system is very limited.";

    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.Exiled;
}