using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.TokenSystem.Tokens.ValueTokens;

public abstract class LiteralValueToken<T> : ValueToken where T : LiteralValue 
{
    private bool _set = false;
    public T ExactValue
    {
        get => _set ? field : throw new AndrzejFuckedUpException($"Value of a {GetType().AccurateName} was not set.");
        protected set
        {
            _set = true;
            field = value;
        }
    } = null!;
    
    public override TryGet<Value> Value() => ExactValue;
    public override TypeOfValue PossibleValues => new TypeOfValue<T>();
    public override bool IsConstant => true;
}