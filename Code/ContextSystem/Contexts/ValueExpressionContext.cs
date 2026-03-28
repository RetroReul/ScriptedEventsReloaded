using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.BaseMethods.Interfaces;
using SER.Code.MethodSystem.BaseMethods.Yielding;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Contexts;

/// <summary>
/// Used to unify method calls, math expressions and property access into a single context that returns a value.
/// </summary>
public class ValueExpressionContext : AdditionalContext
{
    private readonly BaseToken _initial;

    public abstract class Handler
    {
        public abstract TryGet<Value> GetReturnValue();
        public abstract TryAddTokenRes TryAddToken(BaseToken token);
        public abstract Result VerifyCurrentState();
        public abstract IEnumerator<float> Run();
    }

    private readonly string? _error;
    private Handler? _handler;
    
    /// <summary>
    /// Used to unify method calls, math expressions and property access into a single context that returns a value.
    /// </summary>
    public ValueExpressionContext(BaseToken initial, bool allowsYielding, RunnableContext parent)
    {
        _initial = initial;
        try
        {
            if (initial is MethodToken methodToken)
            {
                _handler = new MethodHandler(methodToken, allowsYielding, parent);
            }
            else if (initial is not IValueToken)
            {
                _error = $"{initial} is not a valid way to get a value.";
            }
        }
        catch (Exception e)
        {
            _error = e.Message;
        }
    }

    protected override string FriendlyName => "value expression";
    
    public override TryAddTokenRes TryAddToken(BaseToken token) 
    {
        if (_error is not null) return TryAddTokenRes.Error(_error);

        _handler ??= token switch
        {
            SymbolToken => new GenericValueExpressionResolver(_initial, Script),
            // we assume that an unknown token means property access, but thats kinda sloppy
            _ => new ValuePropertyHandler(_initial, (IValueToken)_initial)
        };
        
        return _handler.TryAddToken(token);
    }

    public override Result VerifyCurrentState()
    {
        if (_error is not null) return _error;
        if (_handler is null) return $"{_initial} is not a valid way to get a value.";
        return _handler.VerifyCurrentState();
    }

    public IEnumerator<float> Run()
    {
        var coro = _handler!.Run();
        while (coro.MoveNext()) yield return coro.Current;
    }
}

public class MethodHandler : ValueExpressionContext.Handler
{
    private readonly RunnableContext _parent;
    private readonly MethodContext _context;

    public MethodHandler(MethodToken token, bool allowsYielding, RunnableContext parent)
    {
        _parent = parent;
        _context = (MethodContext)token.GetContext(token.Script);
        var method = token.Method;
        
        if (method is not IReturningMethod) 
            throw new Exception($"Method '{method.Name}' does not return a value.'");
        
        if (method is YieldingMethod && !allowsYielding)
            throw new Exception(
                $"Method '{method.Name}' is yielding, but you cannot use yielding methods in this context. " +
                $"Consider making a variable and using that variable instead.");
    }

    public override TryGet<Value> GetReturnValue() => _context.ReturnedValue 
                                                      ?? throw new ScriptRuntimeError(_parent, _context.MissingValueHint);

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return _context.TryAddToken(token);
    }

    public override Result VerifyCurrentState()
    {
        return _context.VerifyCurrentState();
    }

    public override IEnumerator<float> Run()
    {
        var coro = _context.Run();
        while (coro.MoveNext()) yield return coro.Current;
    }
}

/// <remarks>
/// Keep in mind that this class will also be used for simple value getting, as parameters are not required!
/// </remarks>
public class ValuePropertyHandler(
    BaseToken baseToken, 
    IValueToken valueToken) : ValueExpressionContext.Handler
{
    private readonly Queue<string> _propertyNames = [];
    private string _valueRepresentation = baseToken.RawRep;
    private TypeOfValue _lastValueType = valueToken.PossibleValues;

    public override TryGet<Value> GetReturnValue()
    {
        if (valueToken.Value().HasErrored(out var error, out var value))
        {
            return $"Failed to get value from '{_valueRepresentation}'".AsError()
                   + error.AsError();
        }

        Value current = value;
        while (_propertyNames.Count > 0)
        {
            var prop = _propertyNames.Dequeue();
            if (!current.Properties.TryGetValue(prop, out var propInfo))
            {
                return $"{value} does not have property '{prop}'.";
            }
            
            current = propInfo.Func(current);
        }
        
        return current;
    }

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        // type verification
        if (_lastValueType.AreKnown(out var types))
        {
            foreach (var type in types)
            {
                if (Value.GetPropertiesOfValue(type).TryGetValue(token.RawRep, out var property))
                {
                    _valueRepresentation += $" {token.RawRep}";
                    _lastValueType = property.ReturnType;
                    break;
                }
            }
            
            return TryAddTokenRes.Error($"'{token.RawRep}' is not a valid property of '{_valueRepresentation}' value.");
        }
        
        _propertyNames.Enqueue(token.RawRep);
        return TryAddTokenRes.Continue();
    }

    public override Result VerifyCurrentState() => true;

    public override IEnumerator<float> Run()
    {
        yield break;
    }
}

/// <summary>
/// Used for math expressions.
/// </summary>
public class GenericValueExpressionResolver(BaseToken initial, Script scr)
    : ValueExpressionContext.Handler
{
    private readonly List<BaseToken> _tokens = [initial];
    private Safe<NumericExpressionReslover.CompiledExpression> _expression;
    
    public override TryGet<Value> GetReturnValue()
    {
        return _expression.Value.Evaluate().OnSuccess(obj => Value.Parse(obj, scr));
    }

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        _tokens.Add(token);
        return TryAddTokenRes.Continue();
    }

    public override Result VerifyCurrentState()
    {
        if (NumericExpressionReslover.CompileExpression(_tokens.ToArray()).HasErrored(out var error, out var compiledExpression))
        {
            return error;
        }
        
        _expression = compiledExpression;
        return true;
    }

    public override IEnumerator<float> Run()
    {
        yield break;
    }
}