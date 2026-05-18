using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.ContextSystem.ValueExpressions;

/// <summary>
///     Used for math expressions.
/// </summary>
public class NumericValueExpression(BaseToken initial, Script scr)
    : ValueExpressionContext.Handler
{
    private readonly List<BaseToken> _tokens = [initial];
    private Safe<NumericExpressionReslover.CompiledExpression> _expression;

    public override string FriendlyName => "numeric expression";

    public override TypeOfValue PossibleValues => new UnknownTypeOfValue();

    public override TryGet<Value> GetReturnValue()
    {
        return _expression.Value.Evaluate() is {} result
            ? Value.Parse(result, scr)
            : new InvalidValue();
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