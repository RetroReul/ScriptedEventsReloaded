using MEC;

namespace SER.Code.Helpers.FrameworkExtensions;

public sealed class CallvoteBridge : FrameworkBridge
{
    public static event Action? OnDetected;
    protected override string Name => "Callvote";
    
    public void Load()
    {
        Await(OnDetected).RunCoroutine();
    }
}