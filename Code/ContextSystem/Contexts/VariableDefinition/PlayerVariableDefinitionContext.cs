using SER.Code.ContextSystem.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.ContextSystem.Contexts.VariableDefinition;

public class PlayerVariableDefinitionContext(VariableToken<PlayerVariable, PlayerValue> varToken) : 
    VariableDefinitionContext<VariableToken<PlayerVariable, PlayerValue>, PlayerValue, PlayerVariable>(varToken)
{
    protected override (TryAddTokenRes result, Func<PlayerValue> parser) AdditionalParsing(BaseToken token)
    {
        if (token is ParenthesesToken { RawContent: "" })
        {
            return (TryAddTokenRes.End(), () => new([]));
        }

        return base.AdditionalParsing(token);
    }
}