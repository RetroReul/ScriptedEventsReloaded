namespace SER.Code.ContextSystem.BaseContexts;

public abstract class StandardContext : RunnableContext
{
    public void Run()
    {
        if (LineNum.HasValue)
        {
            Script.CurrentLine = LineNum.Value;
        }
        
        Execute();
    }
    
    protected abstract void Execute();
}