using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ValueTokens;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using SER.Code.ValueSystem.PropertySystem;

namespace SER.Code.ContextSystem.ValueExpressions;

/// <remarks>
///     Keep in mind that this class will also be used for simple value getting, as parameters are not required!
/// </remarks>
public class PropertyAccessValueExpression(BaseToken baseToken, ValueToken valueToken) 
    : ValueExpressionContext.Handler
{
    private readonly PropertyAccess _propertyAccess = new(baseToken, valueToken);

    public override string FriendlyName => "property access";
    public override TypeOfValue PossibleValues => _propertyAccess.PossibleValues;

    public override TryGet<Value> GetReturnValue()
    {
        return _propertyAccess.ResolveValue();
    }

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return _propertyAccess.TryAddToken(token);
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            _propertyAccess.PropertyCount > 0,
            $"The '{SymbolToken.Arrow}' operator was used, but no property to be accessed was specified."
        );
    }

    public override IEnumerator<float> Run()
    {
        yield break;
    }
}