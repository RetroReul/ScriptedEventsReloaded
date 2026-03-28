using SER.Code.Helpers;
using SER.Code.ScriptSystem;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class AdditionalContext : Context
{
    private Safe<RunnableContext> _parentContext;
    public RunnableContext ParentContext
    {
        get => _parentContext.Value;
        set => _parentContext = value;
    }
    
    public static AdditionalContext Create(Type contextType, Script scr, RunnableContext parentContext)
    {
        var context = (AdditionalContext)Activator.CreateInstance(contextType);
        context.Script = scr;
        context.ParentContext = parentContext;
        return context;
    }

    public override string ToString() => $"{FriendlyName} attached to {ParentContext}";
}