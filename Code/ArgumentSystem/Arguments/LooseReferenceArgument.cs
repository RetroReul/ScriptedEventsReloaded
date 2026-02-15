using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class LooseReferenceArgument(string name, Type type) : Argument(name)
{
    private readonly string _validInput = $"a reference to {type.AccurateName} object.";
    public override string InputDescription => _validInput;

    [UsedImplicitly]
    public virtual DynamicTryGet<object> GetConvertSolution(BaseToken token)
    {
        if (!token.CanReturn<ReferenceValue>(out var get))
        {
            return $"Value '{token.RawRep}' does not represent a valid reference.";
        }

        return new(() => get().OnSuccess(rv => TryParse(rv, type), null));
    }

    public TryGet<object> TryParse(ReferenceValue value, Type targetType)
    {
        if (targetType.IsInstanceOfType(value.Value))
        {
            return value.Value;
        }
        
        return $"The {value} reference is not {_validInput}";
    }
}


public class ReferenceArgument<TValue>(string name) : LooseReferenceArgument(name, typeof(TValue))
{
    private static readonly string ValidInput = $"a reference to {typeof(TValue).AccurateName} object.";
    public override string InputDescription => ValidInput;

    [UsedImplicitly]
    public new DynamicTryGet<TValue> GetConvertSolution(BaseToken token)
    {
        if (!token.CanReturn<ReferenceValue>(out var get))
        {
            return $"Value '{token.RawRep}' does not represent a valid reference.";
        }

        return new(() => get().OnSuccess(TryParse, null));
    }

    public static TryGet<TValue> TryParse(ReferenceValue value)
    {
        if (value.Value is TValue tValue)
        {
            return tValue;
        }
        
        return $"The {value} reference is not {ValidInput}";
    }
}