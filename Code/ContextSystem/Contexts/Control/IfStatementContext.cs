using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Extensions;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

public class IfStatementContext : StatementContext, IExtendableStatement, IKeywordContext
{
    public string KeywordName => "if";
    public string Description => "This statement will execute only if the provided condition is met.";
    public string[] Arguments => ["[condition]"];
    
    public IExtendableStatement.Signal AllowedSignals => IExtendableStatement.Signal.DidntExecute;
    public Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; } = [];

    private readonly List<BaseToken> _condition = [];
    
    private NumericExpressionReslover.CompiledExpression _expression;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        _condition.Add(token);
        return TryAddTokenRes.Continue();
    }

    public override Result VerifyCurrentState()
    {
        if (NumericExpressionReslover.CompileExpression(_condition.ToArray())
            .HasErrored(out var error, out var cond))
        {
            return error;
        }
        
        _expression = cond;
        
        return _condition.Count > 0
            ? true
            : "An if statement expects to have a condition, but none was provided!";
    }

    protected override IEnumerator<float> Execute()
    {
        if (_expression.Evaluate().HasErrored(out var error, out var objResult))
        {
            throw new ScriptRuntimeError(error);
        }

        if (objResult is not bool result)
        {
            throw new ScriptRuntimeError($"An if statement condition must evaluate to a boolean value, but received {objResult.FriendlyTypeName()}");
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
        
        foreach (var child in Children)
        {
            if (!Script.IsRunning)
            {
                yield break;
            }
            
            var coro = child.ExecuteBaseContext();
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