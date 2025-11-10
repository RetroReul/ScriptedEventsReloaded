using SER.ValueSystem;

namespace SER.MethodSystem.Structures;

public class ParseResult(Value? value)
{
    public Value? Value => value;
    public bool Success => value != null;
}

public class ParseResult<T>(T? value) : ParseResult(value) where T : Value
{
    public new T? Value => value;
}