using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.ContextSystem.Contexts.VariableDefinition;

public class ReferenceVariableDefinitionContext(VariableToken<ReferenceVariable, ReferenceValue> varToken) : 
    VariableDefinitionContext<VariableToken<ReferenceVariable, ReferenceValue>, ReferenceValue, ReferenceVariable>(varToken);