using SER.Code.ScriptSystem;

namespace SER.Code.TokenSystem.Tokens;

public class SymbolToken : BaseToken
{
    public bool IsJoker => RawRep == "*";
    
    protected override IParseResult InternalParse(Script scr)
    {
        return RawRep.All(c => char.IsSymbol(c) || char.IsPunctuation(c))
            ? new Success()
            : new Ignore();
    }
}