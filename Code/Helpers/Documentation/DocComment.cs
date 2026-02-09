using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.Helpers.Documentation;

public class DocComment : DocComponent
{
    private readonly string _comment;
    
    public DocComment(string comment)
    {
        _comment = comment;
    }
    
    public DocComment(params object[] parts)
    {
        if (parts.Any(p => p.GetType() != typeof(string) && p is not BaseToken))
        {
            throw new InvalidDocsSymbolException("DocComment.cs");
        }

        _comment = parts.Select(p =>
        {
            if (p is BaseToken t) return t.RawRep;
            return p.ToString();
        }).JoinStrings(" ");
    }

    public override string ToString()
    {
        return $"# {_comment}";
    }
}