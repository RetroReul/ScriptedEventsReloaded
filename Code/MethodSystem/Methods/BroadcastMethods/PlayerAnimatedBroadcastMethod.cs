using Cassie;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.BroadcastMethods;

[UsedImplicitly]
public class PlayerAnimatedBroadcastMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Sends an animated broadcast to specified players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new DurationArgument("duration"),
        new TextArgument("content"),
        new IntArgument("line break length")
        {
            Description = "The maximum amount of characters that can be displayed before making a new line",
            DefaultValue = new(60, null)
        }
    ];

    public override void Execute()
    {
        var content = Args.GetText("content");
        var duration = Args.GetDuration("duration").TotalSeconds;

        foreach (var plr in Args.GetPlayers("players"))
        {
            plr.Connection.Send(new CassieTtsPayload(string.Empty, string.Empty, false));
            plr.SendCassieMessage(
                $"$SLEEP_{duration-1} .",
                AnimatedBroadcastMethod.Helper.FormatToCassieCentralScreenSubtitles(content, Args.GetInt("line break length")),
                false,
                0
            );
        }
    }

    public string AdditionalDescription =>
        "Uses CASSIE to make an animated broadcast - if there is CASSIE playing, it will be stopped. " +
        "Keep custom formatting to a minimum, this system is very limited.";
}