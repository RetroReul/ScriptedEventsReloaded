using SER.Code.Helpers.ResultSystem;

namespace SER.Code.ArgumentSystem.Structures;

public struct ArgumentValueInfo()
{
    public required string Name { get; init; }
    public required Type ArgumentType { get; init; }
    public required DynamicTryGet Evaluator { get; init; }
    public bool IsPartOfCollection { get; init; } = false;
}