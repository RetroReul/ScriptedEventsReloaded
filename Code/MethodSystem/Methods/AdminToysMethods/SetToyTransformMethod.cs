using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.AdminToysMethods;

[UsedImplicitly]
public class SetToyTransformMethod : SynchronousMethod
{
    public override string Description => "Sets the Admin Toy's scale and rotation.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<AdminToy>("toy reference"),
        
        new OptionsArgument("scale mode", "add", "set")
        {
            DefaultValue = new("add", null)
        },
        new FloatArgument("x scale") { DefaultValue = new(0f, null) },
        new FloatArgument("y scale") { DefaultValue = new(0f, null) },
        new FloatArgument("z scale") { DefaultValue = new(0f, null) },
        
        new OptionsArgument("rotation mode", "add", "set")
        {
            DefaultValue = new("add", null)
        },
        new FloatArgument("x rotation") { DefaultValue = new(0f, null) },
        new FloatArgument("y rotation") { DefaultValue = new(0f, null) },
        new FloatArgument("z rotation") { DefaultValue = new(0f, null) },
    ];
    
    public override void Execute()
    {
        var toy = Args.GetReference<AdminToy>("toy reference");
        
        var addScale = Args.GetOption("scale mode") == "add";
        var scale = new Vector3(
            Args.GetFloat("x scale"),
            Args.GetFloat("y scale"),
            Args.GetFloat("z scale"));
        
        var addRotation = Args.GetOption("rotation mode") == "add";
        var rotation = new Vector3(
            Args.GetFloat("x rotation"),
            Args.GetFloat("y rotation"),
            Args.GetFloat("z rotation"));
        

        toy.Scale = addScale ? toy.Scale + scale : scale;
        toy.Rotation = Quaternion.Euler(addRotation ? toy.Rotation.eulerAngles + rotation : rotation);
    }
}