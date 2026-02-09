using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.Documentation;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class StopContext: StandardContext, IKeywordContext
{
    public string KeywordName => "stop";

    public string Description =>
        "Stops the script from executing.";

    public string[] Arguments => [];

    protected override string FriendlyName => "'stop' keyword";

    protected override TryAddTokenRes OnAddingToken(BaseToken token)
    {
        return TryAddTokenRes.Error(
            "'stop' keyword is not expecting any arguments after it.");
    }

    public override Result VerifyCurrentState()
    {
        return true;
    }

    public override DocComponent[] GetExampleUsage()
    {
        throw new NotImplementedException();
    }

    protected override void Execute()
    {
        Script?.Stop(true);
    }
}