using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using Shape = AdminToys.InvisibleInteractableToy.ColliderShape;

namespace SER.Code.MethodSystem.Methods.AdminToyPropertyMethods;

[UsedImplicitly]
public class SetInteractablePropertiesMethod : SynchronousMethod
{
    public override string Description => $"Sets properties of an {nameof(InteractableToy)}.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<InteractableToy>("toy reference"),
        
        new EnumArgument<Shape>("shape")          { DefaultValue = new(null, "not changing") },
        new FloatArgument("interaction duration") { DefaultValue = new(null, "not changing") },
        new BoolArgument("is locked")             { DefaultValue = new(null, "not changing") },
    ];
    
    public override void Execute()
    {
        var toy = Args.GetReference<InteractableToy>("toy reference");
        
        if (Args.GetNullableEnum<Shape>("shape")          is { } shape)         toy.Shape = shape;
        if (Args.GetNullableFloat("interaction duration") is { } duration) toy.InteractionDuration = duration;
        if (Args.GetNullableBool("is locked")             is { } locked)   toy.IsLocked = locked;
    }
}