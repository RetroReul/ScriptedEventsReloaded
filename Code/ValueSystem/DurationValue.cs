using System.Text;
using SER.Code.ValueSystem.PropertySystem;
using ValueType = SER.Code.ValueSystem.Other.ValueType;

namespace SER.Code.ValueSystem;

public class DurationValue(TimeSpan underlyingValue) : LiteralValue<TimeSpan>(underlyingValue), IValueWithProperties
{
    [UsedImplicitly]
    public DurationValue() : this(TimeSpan.Zero) {}

    public static implicit operator DurationValue(TimeSpan value)
    {
        return new(value);
    }
    
    public static implicit operator TimeSpan(DurationValue value)
    {
        return value.UnderlyingValue;
    }

    public override string StringRep
    {
        get
        {
            StringBuilder sb = new();
            if (UnderlyingValue.Hours > 0)
            {
                sb.Append($"{UnderlyingValue.Hours}h ");
            }

            if (UnderlyingValue.Minutes > 0)
            {
                sb.Append($"{UnderlyingValue.Minutes}m ");
            }

            if (UnderlyingValue.Seconds > 0)
            {
                sb.Append($"{UnderlyingValue.Seconds}s ");
            }

            if (UnderlyingValue.Milliseconds > 0)
            {
                sb.Append($"{UnderlyingValue.Milliseconds:D3}ms ");
            }

            if (sb.Length == 0)
            {
                sb.Append("0s ");
            }
            
            return sb.Remove(sb.Length - 1, 1).ToString();
        }
    }

    [UsedImplicitly]
    public new static string FriendlyName => "duration value";

    private class Prop<T>(Func<DurationValue, T> handler, string? description)
        : IValueWithProperties.PropInfo<DurationValue, T>(handler, description) where T : Value;

    public Dictionary<string, IValueWithProperties.PropInfo> Properties { get; } = new()
    {
        ["h"] = new Prop<NumberValue>(d => d.UnderlyingValue.Hours, "Hours component of the duration"),
        ["m"] = new Prop<NumberValue>(d => d.UnderlyingValue.Minutes, "Minutes component of the duration"),
        ["s"] = new Prop<NumberValue>(d => d.UnderlyingValue.Seconds, "Seconds component of the duration"),
        ["ms"] = new Prop<NumberValue>(d => d.UnderlyingValue.Milliseconds, "Milliseconds component of the duration"),
        ["totalH"] = new Prop<NumberValue>(d => (decimal)d.UnderlyingValue.TotalHours, "Total hours in the duration"),
        ["totalM"] = new Prop<NumberValue>(d => (decimal)d.UnderlyingValue.TotalMinutes, "Total minutes in the duration"),
        ["totalS"] = new Prop<NumberValue>(d => (decimal)d.UnderlyingValue.TotalSeconds, "Total seconds in the duration"),
        ["totalMs"] = new Prop<NumberValue>(d => (decimal)d.UnderlyingValue.TotalMilliseconds, "Total milliseconds in the duration"),
        ["valType"] = new Prop<EnumValue<ValueType>>(_ => ValueType.Duration, "The type of the value")
    };
}