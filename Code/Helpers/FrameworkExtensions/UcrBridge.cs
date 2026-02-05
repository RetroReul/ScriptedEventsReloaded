using MEC;

namespace SER.Code.Helpers.FrameworkExtensions;

public sealed class UcrBridge : FrameworkBridge
{
    public static event Action? OnDetected;
    protected override string Name => "UncomplicatedCustomRoles";
    
    public void Load()
    {
        Await(OnDetected).RunCoroutine();
    }
}