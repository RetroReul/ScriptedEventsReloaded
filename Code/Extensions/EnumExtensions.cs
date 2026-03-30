using System;
using SER.Code.ValueSystem;

namespace SER.Code.Extensions;

public static class EnumExtensions
{
    public static EnumValue<T> ToEnumValue<T>(this T enumValue) where T : struct, Enum
    {
        return new EnumValue<T>(enumValue);
    }
    
    public static IEnumerable<T> GetFlags<T>(this T value) where T : struct, Enum
    {
        return from T flag in Enum.GetValues(typeof(T)) 
            where Convert.ToUInt64(value) != 0
            where Convert.ToUInt64(flag) != 0 
            where value.HasFlag(flag) 
            select flag;
    }
}
