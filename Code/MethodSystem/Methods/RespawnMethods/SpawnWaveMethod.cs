using JetBrains.Annotations;
using Respawning;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.RespawnMethods;

[UsedImplicitly]
public class SpawnWaveMethod : SynchronousMethod
{
    public override string Description => "Forces a wave to start.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new WaveArgument("wave")
    ];
    
    public override void Execute()
    {
        WaveManager.Spawn(Args.GetWave("wave"));
    }
}