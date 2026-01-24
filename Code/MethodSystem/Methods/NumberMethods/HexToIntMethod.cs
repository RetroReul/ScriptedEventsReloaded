using System.Globalization;
using Exiled.Permissions.Commands.Permissions;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.NumberMethods;

[UsedImplicitly]
public class HexToIntMethod : ReturningMethod<NumberValue>, ICanError, IAdditionalDescription
{
    public override string Description => "Parses a hexadecimal number back to a number value";

    public string AdditionalDescription => "Do not include the '#' symbol at the start of the text.";

    public string[] ErrorReasons =>
    [
        "The provided string does not represent a hexadecimal number."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("hex number")
    ];

    public override void Execute()
    {
        ReturnValue = int.TryParse(
            Args.GetText("hex number"), 
            NumberStyles.HexNumber, 
            NumberFormatInfo.InvariantInfo, 
            out var result) 
            ? result 
            : throw new ScriptRuntimeError(this, ErrorReasons[0]);
    }
}