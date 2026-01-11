using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.CommunicationInterfaces;
using SER.Code.ContextSystem.Extensions;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ContextSystem.Contexts;

[UsedImplicitly]
public class FunctionDefinitionContext : StatementContext, IKeywordContext, INotRunningContext, IAcceptOptionalVariableDefinitions
{
    public string FunctionName = null!;
    private bool _break = false;
    private VariableToken[] _expectedVariables = [];
    private readonly List<Variable> _localVariables = [];
    
    public string KeywordName => "func";
    public string Description => "Creates a function under a given name.";
    public string[] Arguments => ["[function name]"];
    
    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (token.GetType() != typeof(BaseToken))
        {
            return TryAddTokenRes.Error(
                $"Value '{token.RawRep}' cannot represent a function name, " +
                $"as it was recognized as a {token.FriendlyTypeName()}"
            );
        }
        
        FunctionName = token.RawRep;
        Script.DefineFunction(FunctionName, this);
        return TryAddTokenRes.End();
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            !string.IsNullOrWhiteSpace(FunctionName),
            "Function name was not provided."
        );
    }

    public Result SetOptionalVariables(params VariableToken[] variableTokens)
    {
        _expectedVariables = variableTokens;
        return true;
    }

    public IEnumerator<float> RunProperly(params Value[] values)
    {
        if (LineNum.HasValue)
            Script.CurrentLine = LineNum.Value;

        if (values.Length != _expectedVariables.Length)
        {
            throw new ScriptRuntimeError(
                $"Provided [{values.Length}] values, but [{_expectedVariables.Length}] were expected."
            );
        }

        foreach (var (value, variableToken) in values.Zip(_expectedVariables, (v, t) => (v, t)))
        {
            if (!variableToken.ValueType.IsInstanceOfType(value))
            {
                throw new ScriptRuntimeError(
                    $"Provided variable '{variableToken.Name}' of type '{value.FriendlyTypeName()}' " +
                    $"does not match expected type '{variableToken.ValueType.FriendlyTypeName()}'"
                );
            }
            
            _localVariables.Add(Variable.Create(variableToken.Name, value));
        }
        
        Script.AddVariables(_localVariables.ToArray());
        return Execute();
    }

    protected override void OnReceivedControlMessageFromChild(ParentContextControlMessage msg)
    {
        if (msg == ParentContextControlMessage.Break)
        {
            _break = true;
            return;
        }
        
        SendControlMessage(msg);
    }

    protected override IEnumerator<float> Execute()
    {
        foreach (var coro in Children
             .TakeWhile(_ => Script.IsRunning)
             .Select(child => child.ExecuteBaseContext())
        )
        {
            while (coro.MoveNext())
            {
                if (!Script.IsRunning || _break)
                {
                    goto Exit;
                }
                
                yield return coro.Current;
            }
        }
        
        Exit:
        _localVariables.ForEach(v => Script.RemoveVariable(v));
    }
}