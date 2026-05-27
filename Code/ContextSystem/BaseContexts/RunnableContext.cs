using SER.Code.Extensions;
using SER.Code.ScriptSystem;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class RunnableContext : Context
{
    public required uint? LineNum { get; set; }

    public StatementContext? ParentContext { get; set; } = null;

    public static RunnableContext Create(Type contextType, Script scr, uint? lineNum)
    {
        var context = contextType.CreateInstance<RunnableContext>();
        context.Script = scr;
        context.LineNum = lineNum;
        return context;
    }

    public override string ToString()
    {
        if (LineNum.HasValue)
            return $"{FriendlyName} at line {LineNum}";

        return FriendlyName;
    }
}