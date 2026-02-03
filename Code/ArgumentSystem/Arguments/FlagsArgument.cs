using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.Methods.ParsingMethods;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class FlagsArgument<T>(string name) : Argument(name)
    where T : struct, Enum
{
    public override string InputDescription =>
        $"A reference to {nameof(ToFlagsMethod.Flags)} object with enum values of type '{typeof(T).AccurateName}'";
    
    [UsedImplicitly]
    public DynamicTryGet<T> GetConvertSolution(BaseToken token)
    {
        if (!token.CanReturn<ReferenceValue>(out var func))
        {
            return $"Value '{token.RawRep}' cannot return a reference.";
        }

        return new(() => func().OnSuccess(rv =>
        {
            if (rv.Value is not ToFlagsMethod.Flags flags) 
                return TryGet<T>.Error($"The return value of reference value '{token.RawRep}' is not a flags object.");

            return flags.To<T>();
        }, null));
    }
}