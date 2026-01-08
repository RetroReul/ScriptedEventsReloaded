using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.BaseMethods;

public abstract class LiteralValueReturningMethod : ReturningMethod<LiteralValue>
{
    public abstract Type[] LiteralReturnTypes { get; }
    public override Type[] ReturnTypes => LiteralReturnTypes;
}