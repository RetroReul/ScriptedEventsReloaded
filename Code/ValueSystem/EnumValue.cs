using JetBrains.Annotations;
using SER.Code.Plugin.Commands.HelpSystem;

namespace SER.Code.ValueSystem;

[UsedImplicitly]
public class EnumValue<T> : TextValue where T : struct, Enum
{
    [UsedImplicitly]
    public EnumValue() : this(default) {}

    public EnumValue(T value) : base(value.ToString(), null)
    {
        HelpInfoStorage.UsedEnums.Add(typeof(T));
    }

    [UsedImplicitly]
    public new static string FriendlyName = $"{typeof(T).Name} enum value";
    
    public static implicit operator EnumValue<T>(T value) => new(value);
}
