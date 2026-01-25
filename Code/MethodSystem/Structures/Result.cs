using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Structures;

public class Result(Value? value)
{
    public Value? Value => value;
    public bool Success => value != null;

    public static implicit operator Result(Value? value)
    {
        return new Result(value);
    }
}

public class Result<T>(T? value) : Result(value) where T : Value
{
    public new T? Value => value;
}