using AdminToys;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class EnumTypeArgument(string name, bool flagOnly = true) : Argument(name)
{
    public override string InputDescription => $"A name of an enum type, like {nameof(PrimitiveFlags)}";

    [UsedImplicitly]
    public DynamicTryGet<Type> GetConvertSolution(BaseToken token)
    {
        var name = token.RawRep;
        Type? enumType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == name && t.IsEnum && (!flagOnly || t.IsDefined(typeof(FlagsAttribute), false)));

        return enumType == null 
            ? $"There is no enum type called '{name}' {(flagOnly ? "that is a flag" : "")}."
            : enumType;
    }
}