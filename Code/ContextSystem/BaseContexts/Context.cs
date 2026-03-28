using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class Context
{
    public required Script Script { get; set; } = null!;

    protected abstract string FriendlyName { get; }

    public abstract TryAddTokenRes TryAddToken(BaseToken token);

    public abstract Result VerifyCurrentState();

    public static Context Create(Type contextType, Script scr)
    {
        var context = (Context)Activator.CreateInstance(contextType);
        context.Script = scr;
        return context;
    }

    public abstract override string ToString();
}