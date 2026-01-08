using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Extensions;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

public class ElifStatementContext : StatementContext, IStatementExtender, IExtendableStatement, IKeywordContext
{
    public string KeywordName => "elif";
    public string Description =>
        "If the statement above it didn't execute, 'elif' statement will try to execute if the provided condition is met.";
    public string[] Arguments => ["[condition]"];

    public IExtendableStatement.Signal Extends => IExtendableStatement.Signal.DidntExecute;
    
    public IExtendableStatement.Signal AllowedSignals => IExtendableStatement.Signal.DidntExecute;
    public Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; } = new();

    private readonly List<BaseToken> _condition = [];
    
    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        _condition.Add(token);
        return TryAddTokenRes.Continue();
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            _condition.Count > 0,
            "An elif statement expects to have a condition, but none was provided!");
    }

    protected override IEnumerator<float> Execute()
    {
        if (NumericExpressionReslover.EvalCondition(_condition.ToArray(), Script).HasErrored(out var error, out var result))
        {
            throw new ScriptRuntimeError($"'elif' statement condition error: {error}");
        }
        
        if (!result)
        {
            if (!RegisteredSignals.TryGetValue(IExtendableStatement.Signal.DidntExecute, out var enumerator))
            {
                yield break;
            }
            
            var coro = enumerator();
            while (coro.MoveNext())
            {
                if (!Script.IsRunning)
                {
                    yield break;
                }
                
                yield return coro.Current;
            }
            
            yield break;
        }
        
        foreach (var coro in Children
                     .TakeWhile(_ => Script.IsRunning)
                     .Select(child => child.ExecuteBaseContext()))
        {
            while (coro.MoveNext())
            {
                if (!Script.IsRunning)
                {
                    yield break;
                }
                
                yield return coro.Current;
            }
        }
    }
}