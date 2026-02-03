using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using CapybaraToy = LabApi.Features.Wrappers.CapybaraToy;
using LightSourceToy = LabApi.Features.Wrappers.LightSourceToy;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;
using SpeakerToy = LabApi.Features.Wrappers.SpeakerToy;
using TextToy = LabApi.Features.Wrappers.TextToy;
using WaypointToy = LabApi.Features.Wrappers.WaypointToy;

namespace SER.Code.MethodSystem.Methods.AdminToysMethods;

[UsedImplicitly]
public class CreateToyMethod : ReferenceReturningMethod<AdminToy>, IAdditionalDescription
{
    public override string Description => "Creates an Admin Toy";

    public string AdditionalDescription =>
        $"Remember to set the position if the admin to using methods like {GetFriendlyName(typeof(TPToyPosMethod))}";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("toy type",
            new("primitiveObject", $"Returns a {typeof(PrimitiveObjectToy).AccurateName} reference."),
            new("lightSource", $"Returns a {typeof(LightSourceToy).AccurateName} reference."),
            new("shootingTarget", $"Returns a {typeof(ShootingTargetToy).AccurateName} reference."), 
            new("speaker", $"Returns a {typeof(SpeakerToy).AccurateName} reference."),
            new("interactable", $"Returns a {typeof(InteractableToy).AccurateName} reference."),
            new("camera", $"Returns a {typeof(CameraToy).AccurateName} reference."),
            new("capybara", $"Returns a {typeof(CapybaraToy).AccurateName} reference."), 
            new("text", $"Returns a {typeof(TextToy).AccurateName} reference."),
            new("waypoint", $"Returns a {typeof(WaypointToy).AccurateName} reference.")
        )
    ];

    public override void Execute()
    {
        ReturnValue = Args.GetOption("toy type") switch
        {
            "primitiveobject" => PrimitiveObjectToy.Create(),
            "lightsource"     => LightSourceToy.Create(),
            "shootingtarget"  => ShootingTargetToy.Create(),
            "speaker"         => SpeakerToy.Create(),
            "interactable"    => InteractableToy.Create(),
            "camera"          => CameraToy.Create(),
            "capybara"        => CapybaraToy.Create(),
            "text"            => TextToy.Create(),
            "waypoint"        => WaypointToy.Create(),
            _                 => throw new TosoksFuckedUpException("out of order")
        };
    }
}