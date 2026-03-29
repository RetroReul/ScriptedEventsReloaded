using SER.Code.ContextSystem.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;
using Log = SER.Code.Helpers.Log;

namespace SER.Code.ContextSystem.Contexts.VariableDefinition;

public class PlayerVariableDefinitionContext(VariableToken<PlayerVariable, PlayerValue> varToken) : 
    VariableDefinitionContext<VariableToken<PlayerVariable, PlayerValue>, PlayerValue, PlayerVariable>(varToken);