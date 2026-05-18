using SER.Code.ContextSystem.Contexts;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ValueTokens;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.ContextSystem.ValueExpressions;

public class FunctionCallValueExpression(Script scr) : ValueExpressionContext.Handler
{
    private readonly List<ValueToken> _providedValues = [];
    private FuncStatement? _func;

    public override string FriendlyName => _func!.FriendlyName;

    public override TypeOfValue PossibleValues =>
        _func?.Returns
        ?? throw new AndrzejFuckedUpException("Function has no return type.");

    public override TryGet<Value> GetReturnValue()
    {
        if (_func!.ReturnedValue is { } value) return value;
        return _func.MissingValueHint;
    }

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (_func is null)
        {
            if (!scr.DefinedFunctions.TryGetValue(token.RawRep, out var func))
            {
                return TryAddTokenRes.Error($"Function '{token.RawRep}' is not defined.");
            }

            _func = func;
            return TryAddTokenRes.Continue();
        }

        if (token is ValueToken valToken)
        {
            _providedValues.Add(valToken);
            return TryAddTokenRes.Continue();
        }

        return TryAddTokenRes.Error($"Unexpected token '{token.RawRep}'");
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            _func is not null,
            "Function to run was not provided."
        );
    }

    public override IEnumerator<float> Run()
    {
        List<Value> varsToProvide = [];
        foreach (var valToken in _providedValues)
        {
            if (valToken.Value().HasErrored(out var error, out var variable))
            {
                throw new ScriptRuntimeError(_func!,
                    $"Cannot run {_func!.FriendlyName}: {error}"
                );
            }

            varsToProvide.Add(variable);
        }

        var coro = _func!.RunProperly(varsToProvide.ToArray());
        while (coro.MoveNext()) yield return coro.Current;
    }
}