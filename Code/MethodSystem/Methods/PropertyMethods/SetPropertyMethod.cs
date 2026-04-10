using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.PropertySystem;
using System;
using System.Linq;

namespace SER.Code.MethodSystem.Methods.PropertyMethods;

[UsedImplicitly]
public class SetPropertyMethod : SynchronousMethod
{
    public override string Description => "Sets a property of a reference object.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new LooseReferenceArgument("object", typeof(object)),
        new TextArgument("property name", false),
        new AnyValueArgument("value")
    ];

    public override void Execute()
    {
        var target = Args.GetLooseReference<object>("object");
        var propertyName = Args.GetText("property name");
        var newValue = Args.GetAnyValue("value");

        if (target == null)
            throw new ScriptRuntimeError(this, "Target object is null.");

        var props = ReferencePropertyRegistry.GetProperties(target.GetType());
        if (!props.TryGetValue(propertyName, out var propInfo))
        {
            var writableProps = props
                .Where(p => p.Value.IsSettable)
                .Select(p => p.Key)
                .OrderBy(n => n)
                .ToArray();
                
            var msg = $"Property '{propertyName}' not found on object of type '{target.GetType().Name}'.";
            if (writableProps.Length > 0)
            {
                msg += $" Available writable properties: {string.Join(", ", writableProps)}";
            }
            else
            {
                msg += " This object has no writable properties.";
            }
            throw new ScriptRuntimeError(this, msg);
        }

        if (!propInfo.IsSettable)
        {
            var writableProps = props
                .Where(p => p.Value.IsSettable)
                .Select(p => p.Key)
                .OrderBy(n => n)
                .ToArray();
            
            var msg = $"Property '{propertyName}' is read-only.";
            if (writableProps.Length > 0)
            {
                msg += $" Available writable properties: {string.Join(", ", writableProps)}";
            }
            throw new ScriptRuntimeError(this, msg);
        }

        var result = propInfo.SetValue(target, newValue);
        if (result.HasErrored(out var error))
        {
            throw new ScriptRuntimeError(this, $"Failed to set property '{propertyName}': {error}");
        }
    }
}
