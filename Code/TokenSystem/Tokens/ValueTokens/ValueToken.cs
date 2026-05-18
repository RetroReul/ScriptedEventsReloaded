using SER.Code.Helpers.ResultSystem;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.TokenSystem.Tokens.ValueTokens;

/// <summary>
/// Wrapper for a base token that represents a value.
/// </summary>
public abstract class ValueToken : BaseToken
{
    public abstract TryGet<Value> Value();
    public abstract TypeOfValue PossibleValues { get; }
    public abstract bool IsConstant { get; }
}