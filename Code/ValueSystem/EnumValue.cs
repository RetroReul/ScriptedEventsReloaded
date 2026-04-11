using JetBrains.Annotations;
using SER.Code.Plugin.Commands.HelpSystem;
using ValueType = SER.Code.ValueSystem.Other.ValueType;

namespace SER.Code.ValueSystem;

[UsedImplicitly]
public class EnumValue<T> : TextValue where T : struct, Enum
{
    public override ValueType ValType => ValueType.Enum;

    [UsedImplicitly]
    public EnumValue() : this(default) {}

    public EnumValue(T value) : base(value.ToString(), null)
    {
        HelpInfoStorage.UsedEnums.Add(typeof(T));
    }

    [UsedImplicitly]
    public new static string FriendlyName = $"{typeof(T).Name} enum value";
}
