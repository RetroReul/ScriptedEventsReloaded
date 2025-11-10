using SER.ValueSystem;

namespace SER.MethodSystem.BaseMethods;

public abstract class ReturningMethod : SynchronousMethod 
{
    public Value? ReturnValue { get; protected set; }
    public abstract Type[]? ReturnTypes { get; }
}

public abstract class ReturningMethod<T> : ReturningMethod
    where T : Value
{
    public override Type[] ReturnTypes => [typeof(T)];

    protected new T? ReturnValue
    {
        set => base.ReturnValue = value;
    }
}