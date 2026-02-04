using Log = SER.Code.Helpers.Log;
using Logger = LabApi.Features.Console.Logger;

namespace SER.Code.ScriptSystem.Structures;

public class ServerConsoleExecutor : ScriptExecutor
{
    private ServerConsoleExecutor()
    {
    }
    
    public static ServerConsoleExecutor Instance { get; } = new();

    public override void Reply(string content, Script scr)
    {
        Logger.Raw($"[Script '{scr.Name}'] {content}", ConsoleColor.Green);
    }

    public override void Warn(string content, Script scr)
    {
        Log.ScriptWarn(scr, content);
    }

    public override void Error(string content, Script scr)
    {
        if (scr.CurrentLine == 0)
        {
            Log.CompileError(scr.Name, content);
        }
        else
        {
            Log.RuntimeError(scr.Name, scr.CurrentLine, content);
        }
    }
}