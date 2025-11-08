using JetBrains.Annotations;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.ResultSystem;
using SER.ScriptSystem;
using SER.TokenSystem.Tokens;

namespace SER.ArgumentSystem.Arguments;

public class RunningScriptArgument(string name) : Argument(name)
{
    public override string InputDescription => "Name of a currently running script";

    [UsedImplicitly]
    public DynamicTryGet<Script> GetConvertSolution(BaseToken token)
    {
        return new(() =>
        {
            var name = token.GetBestTextRepresentation(Script);
            if (Script.RunningScripts.FirstOrDefault(scr => scr.Name == name) is not {} runningScript)
            {
                return $"There is no running script named '{name}'";
            }
            
            return runningScript;
        });
    }
}