using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Extensions;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class ForeverLoopContext : LoopContext, IKeywordContext
{
    private readonly Result _mainErr = "Cannot create 'forever' loop.";

    public override Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; } =
        new();
    
    public override string KeywordName => "forever";
    public override string Description => "Makes the code inside the statement run indefinitely.";
    public override string[] Arguments => [];

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return TryAddTokenRes.Error(_mainErr + "'forever' loop doesn't expect any arguments.");
    }

    public override Result VerifyCurrentState()
    {
        return true;
    }

    protected override IEnumerator<float> Execute()
    {
        while (true)
        {
            foreach (var coro in Children.Select(child => child.ExecuteBaseContext()))
            {
                if (ExitLoop) yield break;
                
                while (coro.MoveNext())
                {
                    yield return coro.Current;
                }

                if (!SkipThisIteration) continue;

                SkipThisIteration = false;
                break;
            }
        }
    }
}