using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;

namespace SER.Code.ValueSystem;

public abstract class LiteralValue(object value) : Value
{
    public abstract string StringRep { get; }

    public object Value => value;

    public override bool EqualCondition(Value other) => other is LiteralValue otherP && Value.Equals(otherP.Value);

    public override string ToString()
    {
        return StringRep;
    }

    public TryGet<T> TryGetValue<T>() where T : Value
    {
        if (this is T tValue)
        {
            return tValue;
        }

        return $"Value is not of type {typeof(T).FriendlyTypeName()}, but {Value.FriendlyTypeName()}.";
    }

    public override int HashCode => Value.GetHashCode();
}

public abstract class LiteralValue<T>(T value) : LiteralValue(value)
    where T : notnull
{
    public new T Value => value;
}