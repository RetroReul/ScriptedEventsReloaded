using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.MethodSystem.Methods.TimeMethods;

[UsedImplicitly]
public class TimeInfoMethod : LiteralValueReturningMethod
{
    public override string Description => "Returns information about current date and time.";

    public override TypeOfValue LiteralReturnTypes => new TypesOfValue(
        typeof(NumberValue), 
        typeof(TextValue), 
        typeof(EnumValue<DayOfWeek>));
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("options",
            "second",
            "minute",
            "hour",
            "month",
            "year",
            Option.Enum<DayOfWeek>("dayOfWeek"),
            new("dayOfWeekNumber", "Instead of returning e.g. 'Monday', will return 1"),
            "dayOfMonth",
            "dayOfYear",
            new("unix","Useful for making discord timestamps - always uses UTC")),
        new OptionsArgument("time zone",
            "utc",
            "local")
        {
            DefaultValue = new("local", null)
        }
    ];
    
    public override void Execute()
    {
        var dateTime = Args.GetOption("time zone") is "utc"
            ? DateTime.UtcNow
            : DateTime.Now;
        
        ReturnValue = Args.GetOption("options") switch
        {
            "second" => new NumberValue(dateTime.Second),
            "minute" => new NumberValue(dateTime.Minute),
            "hour" => new NumberValue(dateTime.Hour),
            "month" => new NumberValue(dateTime.Month),
            "year" => new NumberValue(dateTime.Year),
            "dayofweek" => dateTime.DayOfWeek.ToEnumValue(),
            "dayofweeknumber" => (uint)dateTime.DayOfWeek == 0
                ? new NumberValue(7)
                : new NumberValue((uint)dateTime.DayOfWeek),
            "dayofmonth" => new NumberValue(dateTime.Day),
            "dayofyear" => new NumberValue(dateTime.DayOfYear),
            "unix" => new NumberValue(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            _ => throw new AndrzejFuckedUpException()
        };
    }
}