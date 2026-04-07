using CustomPlayerEffects;
using JetBrains.Annotations;
using Respawning.Waves;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class WaveTypeArgument(string name) : Argument(name)
{
    public static readonly Type[] WaveTypes = typeof(SpawnableWaveBase).Assembly.GetTypes()
        .Where(t => 
            t.IsSubclassOf(typeof(SpawnableWaveBase)) && 
            !t.IsAbstract
        )
        .ToArray();

    public override string InputDescription => 
        "One of the following wave types:\n" 
        + WaveTypes.Select(t => $"> {t.Name.LowerFirst()}").JoinStrings("\n"); 
    
    [UsedImplicitly]
    public DynamicTryGet<SpawnableWaveBase> GetConvertSolution(BaseToken token)
    {
        if (!InternalConvert(token.GetBestTextRepresentation(Script)).HasErrored(out var error, out var type))
        {
            return type;
        }

        if (token is not IValueToken valToken || !valToken.CapableOf<LiteralValue>(out var get))
        {
            return error;
        }

        return new(get().OnSuccess(lVal => InternalConvert(lVal.StringRep)));
    }

    private static TryGet<SpawnableWaveBase> InternalConvert(string name)
    {
        if (WaveTypes.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)) is {} type)
        {
            return type.CreateInstance<SpawnableWaveBase>();
        }
        
        return "Value is not a valid wave type.";
    }
}