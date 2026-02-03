using LabApi.Features.Wrappers;

namespace SER.Code.ScriptSystem.Structures;

public class PlayerConsoleExecutor : ScriptExecutor, IPlayerExecutor
{
    public required ReferenceHub Sender { get; init; }

    public Player Player => Player.Get(Sender);

    public override void Reply(string content, Script scr)
    {
        Sender.gameConsoleTransmission.SendToClient(content, "green");
    }

    public override void Warn(string content, Script scr)
    {
        Sender.gameConsoleTransmission.SendToClient(
            $"[WARN] " +
            $"[Script {scr.Name}] " +
            $"[{(scr.CurrentLine == 0 ? "Compile warning" : $"Line {scr.CurrentLine}")}] " +
            $"{content}",
            "yellow"
        );
    }

    public override void Error(string content, Script scr)
    {
        Sender.gameConsoleTransmission.SendToClient(
            $"[ERROR] " +
            $"[Script {scr.Name}] " +
            $"[{(scr.CurrentLine == 0 ? "Compile error" : $"Line {scr.CurrentLine}")}] " +
            $"{content}",
            "red"
        );
    }
}