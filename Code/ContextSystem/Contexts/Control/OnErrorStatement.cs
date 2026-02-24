using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class OnErrorStatement : StatementContext, IStatementExtender, IKeywordContext, IAcceptOptionalVariableDefinitionsContext
{
    public string KeywordName => "on_error";
    public string Description => "Catches an exception thrown inside of a " +
                                 typeof(AttemptStatement).FriendlyTypeName(true);
    public string[] Arguments => [];
    public string? Example =>
        """
        &collection = EmptyCollection
        attempt
            Print {CollectionFetch &collection 2}
            # throws because there's nothing at index 2
        on_error
            with *exception
            
            Print "Error!: {ExceptionInfo *exception message}"
        end
        """;

    public IExtendableStatement.Signal Extends => IExtendableStatement.Signal.ThrewException;
    protected override string FriendlyName => "'on_error' statement";

    public Exception? Exception
    {
        get;
        set
        {
            if (field is not null)
                return;
            field = value;
        }
    }
    private VariableToken? _variableToken;
    private Variable? _variable;
    
    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return TryAddTokenRes.Error($"A {FriendlyName} does not expect any arguments.");
    }

    public override Result VerifyCurrentState()
    {
        return true;
    }

    public Result SetOptionalVariables(params VariableToken[] variableTokens)
    {
        if (variableTokens.Length > 1)
            return $"Too many arguments provided for {FriendlyName}, only 1 is allowed.";
        var token = variableTokens.FirstOrDefault();
        if (token is null) return true;

        if (token is not ReferenceVariableToken)
            return $"Variable {token.RawRepr} cannot be used for a {FriendlyName} as it's not a " +
                   $"{typeof(ReferenceVariable).FriendlyTypeName(true)}.";
        
        _variableToken = token;
        return true;
    }
    
    protected override IEnumerator<float> Execute()
    {
        if (_variableToken is not null)
        {
            _variable = Variable.Create(_variableToken.Name, Value.Parse(Exception!, Script));
            Script.AddLocalVariable(_variable);
        }

        var coro = RunChildren();
        while (coro.MoveNext())
            yield return coro.Current;
        
        if (_variable is not null) Script.RemoveLocalVariable(_variable);
    }
}