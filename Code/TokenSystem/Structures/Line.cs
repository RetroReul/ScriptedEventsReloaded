using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.TokenSystem.Structures;

public class Line
{
    public required uint LineNumber { get; init; }
    public required string RawRepresentation { get; init; }
    public Slice[] Slices = [];
    public BaseToken[] Tokens = [];
}