using CustomPlayerEffects;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class EffectTypeArgument(string name) : Argument(name)
{
    public static readonly Type[] EffectTypes = typeof(StatusEffectBase).Assembly.GetTypes()
        .Where(t => 
            t.IsSubclassOf(typeof(StatusEffectBase)) && 
            !t.IsAbstract && 
            !typeof(IHolidayEffect).IsAssignableFrom(t)
        )
        .ToArray();
    
    public static readonly Dictionary<string, Type> EffectNames = EffectTypes
        .ToDictionary(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase);

    public override string InputDescription => 
        "One of the following effects:\n" 
        + EffectNames.Keys.Select(n => $"> {n}").JoinStrings("\n"); 
    
    [UsedImplicitly]
    public DynamicTryGet<Type> GetConvertSolution(BaseToken token)
    {
        if (!InternalConvert(token.GetBestTextRepresentation(Script)).HasErrored(out var error, out var type))
        {
            return type;
        }

        if (!token.CanReturn<LiteralValue>(out var get))
        {
            return error;
        }

        return new(get().OnSuccess(lVal => InternalConvert(lVal.StringRep)));
    }

    private static TryGet<Type> InternalConvert(string name)
    {
        if (EffectNames.TryGetValue(name, out var type))
        {
            return type;
        }
        
        return "Value is not a valid effect name.";
    }
}