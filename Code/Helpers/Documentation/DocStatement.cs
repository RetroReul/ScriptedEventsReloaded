using SER.Code.Extensions;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.Helpers.Documentation;

public class DocStatement : DocComponent
{
    private readonly string _start;
    private readonly List<DocComponent> _children = [];
    private readonly bool _isStandalone;

    public DocStatement AddRangeIf(Func<DocComponent[]?> children)
    {
        if (children.Invoke() is {} items)
            _children.AddRange(items);
        return this;
    }
    
    public DocStatement AddIf(Func<DocComponent?> child)
    {
        if (child.Invoke() is {} item)
            _children.Add(item);
        return this;
    }
    
    public DocStatement Add(DocComponent? child)
    {
        if (child is not null)
            _children.Add(child);
        return this;
    }
    
    public DocStatement AddRange(DocComponent?[] children)
    {
        _children.AddRange(children.RemoveNulls());
        return this;
    }
    
    public DocStatement(string keyword, bool isStandalone, params BaseToken[] init)
    {
        _isStandalone = isStandalone;
        _start = BaseToken.GetToken<KeywordToken>(keyword).RawRep 
                 + " " 
                 + init.Select(t => t.RawRep).JoinStrings(" ");
    }

    public override string ToString()
    {
        return $"{_start}\n{_children.Select(c => $"    {c}").JoinStrings("\n")}"
            + (_isStandalone ? "" : "\nend" );
    }
}