using System;
using SER.Code.ValueSystem;

namespace SER.Code.Extensions;

public static class EnumExtensions
{
    public static EnumValue<T> ToEnumValue<T>(this T enumValue) where T : struct, Enum
    {
        return new EnumValue<T>(enumValue);
    }
}
