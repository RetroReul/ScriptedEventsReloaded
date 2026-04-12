using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class CustomRoleArgument(string name) : Argument(name)
{
    public override string InputDescription => "Custom role id e.g. myCustomRole";
    
    [UsedImplicitly]
    public DynamicTryGet<CustomRole> GetConvertSolution(BaseToken token)
    {
        if (CustomRole.RegisteredRoles.TryGetValue(token.GetBestTextRepresentation(Script), out var cr))
        {
            return cr;
        }

        if (token is not LiteralVariableToken varToken)
        {
            return $"Provided value '{token.RawRep}' is not a valid custom role id.";
        }

        return new(() =>
        {
            if (varToken.ExactValue.HasErrored(out var error, out var value) 
                || CustomRole.RegisteredRoles.TryGetValue(value.StringRep, out cr))
            {
                return $"Provided value in '{varToken.RawRep}' is not a valid custom role id.".AsError();
            }

            return cr;
        });
    }
}