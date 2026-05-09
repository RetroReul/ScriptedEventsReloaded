using Respawning;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.RespawnMethods;

[UsedImplicitly]
public class PlayWaveEffectMethod : SynchronousMethod
{
    public override string Description => "Plays a Respawn Wave effect (the NTF helicopter/CI van arrival animation)";

    public override Argument[] ExpectedArguments { get; } =
    [
        new WaveArgument("wave type")
    ];
    
    public override void Execute()
    {
        if (Args.GetWave("wave type") is not { Base: { } wave })
        {
            return;
        }
        
        WaveUpdateMessage.ServerSendUpdate(wave, UpdateMessageFlags.Trigger);
    }
}