using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Extensions;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class WhileLoopContext : LoopContext, IExtendableStatement
{
    private readonly Result _rs = "Cannot create 'while' loop.";
    private readonly List<BaseToken> _condition = []; 
    private NumericExpressionReslover.CompiledExpression _expression;
    private bool _skipChild = false;
    
    public override string KeywordName => "while";
    public override string Description =>
        "A statement which will execute its body as long as the provided condition is evaluated to true.";
    public override string[] Arguments => ["[condition...]"];

    public override Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; } = [];

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
        
        return Result.Assert(
            _condition.Count > 0,
            _rs + "The condition was not provided.");
    }

    protected override IEnumerator<float> Execute()
    {
        while (GetExpressionResult())
        {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var child in Children)
            {
                var coro = child.ExecuteBaseContext();
                while (coro.MoveNext())
                {
                    yield return coro.Current;
                }

                if (!_skipChild) continue;

                _skipChild = false;
                break;
            }
            
        }

        if (RegisteredSignals.TryGetValue(IExtendableStatement.Signal.EndedExecution, out var coroFunc))
        {
            var coro = coroFunc();
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


    protected override void OnReceivedControlMessageFromChild(ParentContextControlMessage msg)
    {
        if (msg == ParentContextControlMessage.LoopContinue)
        {
            _skipChild = true;
            return;
        }

        ParentContext?.SendControlMessage(msg);
    }

    private bool GetExpressionResult()
    {
        if (_expression.Evaluate().HasErrored(out var error, out var objResult))
        {
            throw new ScriptRuntimeError(error);
        }

        if (objResult is not bool result)
        {
            throw new ScriptRuntimeError($"A while statement condition must evaluate to a boolean value, but received {objResult.FriendlyTypeName()}");
        }

        return result;
    }
}