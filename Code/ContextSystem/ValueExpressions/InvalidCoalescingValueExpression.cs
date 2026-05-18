using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ValueTokens;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.ContextSystem.ValueExpressions;

public class InvalidCoalescingValueExpression(ValueToken initial) : ValueExpressionContext.Handler
{
    private readonly List<ValueToken> _tokens = [initial];
    private bool _lastTokenCoalescing = false;
    private TypeOfValue? _valueType = null;
    
    public override string FriendlyName => "invalid coalescing expression";
    
    public override TypeOfValue PossibleValues => _valueType ?? new UnknownTypeOfValue();
    
    public override TryGet<Value> GetReturnValue()
    {
        Value? lastValue = null;
        foreach (var token in _tokens)
        {
            if (token.Value().HasErrored(out var error, out lastValue))
            {
                return error;
            }

            if (lastValue.IsInvalid())
            {
                continue;
            }

            return lastValue;
        }
        
        return lastValue ?? throw new AndrzejFuckedUpException("tokens is empty?");
    }
    
    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        Log.Signal(token);
        if (!_lastTokenCoalescing)
        {
            if (token is SymbolToken { IsCoalescing: true })
            {
                Log.Signal("coalescing operator");
                _lastTokenCoalescing = true;
                return TryAddTokenRes.Continue();
            }

            return TryAddTokenRes.Error(
                $"Required coalescing operator '{SymbolToken.Coalescing}' was not found.");
        }
        
        if (token is not ValueToken valToken)
        {
            return TryAddTokenRes.Error($"{token} does not hold a value.");
        }

        if (_valueType is null)
        {
            _valueType = valToken.PossibleValues;
        }
        else
        {
            _valueType |= valToken.PossibleValues;
        }
        
        Log.Signal($"adding token {_valueType}");
        _tokens.Add(valToken);
        _lastTokenCoalescing = false;
        return TryAddTokenRes.Continue();
    }
    
    public override Result VerifyCurrentState()
    {
        return Result.Assert(!_lastTokenCoalescing,
            $"You have a trailing {SymbolToken.Coalescing} operator.");
    }
    
    public override IEnumerator<float> Run()
    {
        yield break;
    }
}