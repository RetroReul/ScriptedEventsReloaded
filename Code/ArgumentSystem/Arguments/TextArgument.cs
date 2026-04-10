using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.TokenSystem.Tokens.ValueTokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class TextArgument(string name, bool allowsSpaces = true) : Argument(name)
{
    public override string InputDescription => allowsSpaces
        ? "Any text e.g. \"Hello, World!\""
        : "Text with no spaces and no quotes e.g. someOption";

    [UsedImplicitly]
    public DynamicTryGet<string> GetConvertSolution(BaseToken token)
    {
        if (token is TextToken textToken)
        {
            return new(() => textToken.GetDynamicResolver().Invoke());
        }
        
        if (token is not IValueToken valToken || !valToken.CapableOf<LiteralValue>(out var get))
        {
            if (!allowsSpaces)
            {
                return token.GetBestTextRepresentation(null).AsSuccess();
            }
            
            return DynamicTryGet.Error("Value cannot represent text.");
        }
        
        if (valToken.IsConstant)
        {
            return get().OnSuccess(v => v.StringRep);
        }

        return new(() => get().OnSuccess(v => v.StringRep));
        
    }
}