using MapGeneration;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.BaseArguments;

public abstract class EnumHandlingArgument(string name) : Argument(name)
{
    public DynamicTryGet<T> ResolveEnums<T>(
        BaseToken token,
        Dictionary<Type, Func<object, TryGet<T>>> handlers,
        Func<DynamicTryGet<T>> fallback)
    {
        foreach (var enumType in handlers.Keys)
        {
            if (EnumArgument< /* dummy type */ RoomName>.Convert(token, Script, enumType).HasErrored(out _, out var enumValue))
            {
                continue;
            }

            return handlers[enumType](enumValue);
        }

        return fallback();
    }
}