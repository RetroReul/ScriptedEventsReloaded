namespace SER.Code.Helpers.Extensions;

public static class TimeSpanExtensions
{
    public static float ToFloatSeconds(this TimeSpan value)
    {
        return (float)value.TotalSeconds;
    }
}