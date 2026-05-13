using SER.Code.ContextSystem.Extensions;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class LoopContext : StatementContext, IExtendableStatement, IKeywordContext
{
    protected bool ReceivedBreak;

    protected bool ReceivedContinue;

    protected abstract string? Usage { get; }

    public sealed override string FriendlyName => $"'{KeywordName}' loop statement";
    public IExtendableStatement.Signal AllowedSignals => IExtendableStatement.Signal.DidntExecute;
    public Dictionary<IExtendableStatement.Signal, StatementContext> RegisteredSignals { get; } = [];

    public abstract string KeywordName { get; }
    public abstract string Description { get; }
    public abstract string[] Arguments { get; }

    public string Example => ExampleHandler.GetExample($"{KeywordName}KeywordExample") ??
                             $"""
                              {Usage}

                              # ========================================
                              # "break" and "continue" keywords work as usual and you are free to use them inside "{KeywordName}" loops
                              """;

    protected override void OnReceivedControlMessageFromChild(ParentContextControlMessage msg)
    {
        switch (msg)
        {
            case Continue:
                ReceivedContinue = true;
                return;
            case Break:
                ReceivedBreak = true;
                return;
            default:
                ParentContext?.SendControlMessage(msg);
                break;
        }
    }

    protected IEnumerator<float> RunChildren()
    {
        foreach (var coro in Children
                     .TakeWhile(_ => !ReceivedBreak)
                     .Select(child => child.ExecuteBaseContext()))
        {
            while (coro.MoveNext())
            {
                yield return coro.Current;
            }

            if (!ReceivedContinue) continue;

            ReceivedContinue = false;
            break;
        }

        WipeEphemeralVariables();
    }
}