using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class Context
{
    public string Name => GetType().FriendlyTypeName();
    
    public required Script Script { get; set; } = null!;
    
    public required uint? LineNum { get; set; }

    public StatementContext? ParentContext { get; set; } = null;

    public abstract TryAddTokenRes TryAddToken(BaseToken token);

    public abstract Result VerifyCurrentState();

    public static Context Create(Type contextType, (Script scr, uint? lineNum) info)
    {
        var context = (Context)Activator.CreateInstance(contextType);
        context.Script = info.scr;
        context.LineNum = info.lineNum;
        return context;
    }

    public override string ToString()
    {
        return Name;
    }
}