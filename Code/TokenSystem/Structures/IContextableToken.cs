using SER.Code.ContextSystem.BaseContexts;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;

namespace SER.Code.TokenSystem.Structures;

public interface IContextableToken
{
    public TryGet<Context> TryGetContext(Script scr);
}