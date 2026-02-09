using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.Helpers.Documentation;

public class DocLine : DocComponent
{
    private readonly string _line;
    
    public DocLine(params BaseToken[] tokens)
    {
        _line = tokens.Select(t => t.RawRep).JoinStrings(" ");
        if (Script.VerifyContent(_line).HasErrored(out var error))
        {
            throw new InvalidDocsSymbolException(error);
        }
    }

    public override string ToString() => _line;
}