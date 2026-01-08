using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class StatementContext : YieldingContext
{
    public readonly List<Context> Children = [];
    
    public void SendControlMessage(ParentContextControlMessage msg)
    {
        Log.Debug($"{Name} context has received control message: {msg}");
        OnReceivedControlMessageFromChild(msg);
    }

    protected virtual void OnReceivedControlMessageFromChild(ParentContextControlMessage msg)
    {
        ParentContext?.SendControlMessage(msg);
    }
}