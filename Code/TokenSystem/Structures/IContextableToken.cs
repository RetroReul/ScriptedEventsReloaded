using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ScriptSystem;

namespace SER.Code.TokenSystem.Structures;

public interface IContextableToken
{
    public RunnableContext? GetContext(Script scr);
}