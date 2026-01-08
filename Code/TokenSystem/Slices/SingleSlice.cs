using SER.Code.Helpers.ResultSystem;

namespace SER.Code.TokenSystem.Slices;

public class SingleSlice(char startChar) : Slice(startChar)
{
    public override string Value => RawRep;
    
    public override bool CanContinueAfterAdd(char c)
    {
        if (char.IsWhiteSpace(c)) return false;
        PrivateRawRepresentation.Append(c);
        return true;
    }

    public override Result VerifyState()
    {
        return true;
    }
}