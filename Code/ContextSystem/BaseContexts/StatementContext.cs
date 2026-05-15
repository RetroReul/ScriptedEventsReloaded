using SER.Code.ContextSystem.Extensions;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class StatementContext : YieldingContext
{
    public readonly List<RunnableContext> Children = [];
    public readonly HashSet<Variable> EphemeralVariables = [];
    public uint? EndLine;

    public void SendControlMessage(ParentContextControlMessage msg)
    {
        Log.Debug($"{this} has received control message: {msg}");
        OnReceivedControlMessageFromChild(msg);
    }

    protected virtual void OnReceivedControlMessageFromChild(ParentContextControlMessage msg)
    {
        ParentContext?.SendControlMessage(msg);
    }

    protected IEnumerator<float> RunChildren(Func<bool>? endCond = null)
    {
        foreach (var coro in Children.Select(c => c.ExecuteBaseContext()))
        {
            if (endCond?.Invoke() is true) goto leave;
            while (coro.MoveNext())
            {
                if (endCond?.Invoke() is true) goto leave;
                yield return coro.Current;
            }
        }

        leave:
        WipeEphemeralVariables();
    }

    public void MarkVariableAsEphemeral(Variable variable)
    {
        EphemeralVariables.Add(variable);
    }

    protected void WipeEphemeralVariables()
    {
        if (EphemeralVariables.Count is 0) return;
        
        foreach (var variable in EphemeralVariables)
        {
            Script.RemoveLocalVariable(variable);
        }
        
        EphemeralVariables.Clear();
    }

    protected override void OnEndedExecution()
    {
        WipeEphemeralVariables();
    }
}