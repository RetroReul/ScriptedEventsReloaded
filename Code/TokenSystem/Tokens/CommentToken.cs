using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Structures;

namespace SER.Code.TokenSystem.Tokens;

public class CommentToken : BaseToken, IContextableToken
{
    protected override IParseResult InternalParse(Script scr)
    {
        return RawRep.FirstOrDefault() == '#' 
            ? new Success() 
            : new Ignore();
    }

    public RunnableContext? GetContext(Script scr) => null;
}