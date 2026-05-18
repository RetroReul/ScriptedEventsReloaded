using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ValueTokens;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.ContextSystem.ValueExpressions;

/// <summary>
///     Used to unify method calls, math expressions and property access into a single context that returns a value.
/// </summary>
public class ValueExpressionContext : AdditionalContext
{
    private readonly string? _error;

    private readonly BaseToken _initial;
    private readonly ValueToken? _initialValueToken;
    private Handler? _handler;

    /// <summary>
    ///     Used to unify method calls, math expressions and property access into a single context that returns a value.
    /// </summary>
    public ValueExpressionContext(BaseToken initial, bool allowsYielding)
    { _initial = initial;
        try
        {
            switch (initial)
            {
                case MethodToken methodToken:
                    _handler = new MethodValueExpression(methodToken, allowsYielding, initial.Script);
                    break;
                case RunFunctionToken:
                    _handler = new FunctionCallValueExpression(initial.Script);
                    break;
                case ValueToken valToken:
                    _initialValueToken = valToken;
                    break;
                default:
                    _error = $"{initial} is not a valid way to get a value.";
                    break;
            }
        }
        catch (Exception e)
        {
            _error = e.Message;
        }
    }

    public override string FriendlyName => _handler?.FriendlyName ?? "value expression";

    public TypeOfValue PossibleValues => _handler?.PossibleValues ?? new UnknownTypeOfValue();

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (_error is not null)
        {
            return TryAddTokenRes.Error(_error);
        }

        if (_handler is not null)
        {
            goto try_add_token;
        }

        if (token is not SymbolToken symbol)
        {
            _handler = new NumericValueExpression(_initial, Script);
        }
        else _handler = symbol switch
        {
            { IsCoalescing: true } when _initial is ValueToken valToken => new InvalidCoalescingValueExpression(valToken),
            { IsArrow: true } => new PropertyAccessValueExpression(_initial, (ValueToken)_initial),
            _ => new NumericValueExpression(_initial, Script),
        };

        try_add_token:
        return _handler.TryAddToken(token);
    }

    public override Result VerifyCurrentState()
    {
        if (_error is not null) return _error;
        if (_handler is null) return true;
        return _handler.VerifyCurrentState();
    }

    /// <summary>
    ///     If the context is not yielding, disregard the return value.
    /// </summary>
    public IEnumerator<float> Run()
    {
        if (_handler is null) yield break;
        var coro = _handler.Run();
        while (coro.MoveNext()) yield return coro.Current;
    }

    public TryGet<Value> GetValue()
    {
        return _handler?.GetReturnValue()
               ?? _initialValueToken?.Value()
               ?? throw new AndrzejFuckedUpException();
    }

    public abstract class Handler
    {
        public abstract string FriendlyName { get; }
        public abstract TypeOfValue PossibleValues { get; }
        public abstract TryGet<Value> GetReturnValue();
        public abstract TryAddTokenRes TryAddToken(BaseToken token);
        public abstract Result VerifyCurrentState();
        public abstract IEnumerator<float> Run();
    }
}