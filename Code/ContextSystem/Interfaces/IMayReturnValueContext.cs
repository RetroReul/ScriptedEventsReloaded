using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Interfaces;

public interface IMayReturnValueContext
{
    public TypeOfValue? Returns { get; }
    
    public Value? ReturnedValue { get; }
}