using System;
using JetBrains.Annotations;

namespace SER.Code.ValueSystem;

[UsedImplicitly]
public class EnumValue<T>(T value) : StaticTextValue(value.ToString()) where T : struct, Enum
{
    public T EnumValueObject { get; } = value;

    [UsedImplicitly]
    public EnumValue() : this(default) {}

    [UsedImplicitly]
    public new static string FriendlyName = $"enum value of {typeof(T).Name}";
}
