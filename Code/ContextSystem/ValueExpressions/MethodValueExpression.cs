using SER.Code.ContextSystem.Contexts;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.BaseMethods.Interfaces;
using SER.Code.MethodSystem.BaseMethods.Yielding;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.ContextSystem.ValueExpressions;

public class MethodValueExpression : ValueExpressionContext.Handler
{
    private readonly MethodContext _context;

    public MethodValueExpression(MethodToken token, bool allowsYielding, Script scr)
    {
        _context = new MethodContext(token)
        {
            Script = scr,
            LineNum = null
        };
        var method = token.Method;

        if (method is not IReturningMethod)
            throw new Exception($"Method '{method.Name}' does not return a value.'");

        if (method is YieldingMethod && !allowsYielding)
            throw new Exception(
                $"Method '{method.Name}' is yielding, but you cannot use yielding methods in this context. " +
                $"Consider making a variable and using that variable instead.");
    }

    public override string FriendlyName => _context.FriendlyName;

    public override TypeOfValue PossibleValues =>
        _context.Returns
        ?? throw new AndrzejFuckedUpException("Method has no return type.");

    public override TryGet<Value> GetReturnValue()
    {
        if (_context.ReturnedValue is { } value) return value;
        return _context.MissingValueHint;
    }

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