using LabApi.Features.Console;
using LabApi.Loader;
using MEC;

namespace SER.Code.Helpers.FrameworkExtensions;

public abstract class FrameworkBridge
{
    protected abstract string Name { get; }

    protected IEnumerator<float> Await(Action? onDetected)
    {
        uint attempts = 0;
        while (PluginLoader.EnabledPlugins.All(plg => plg.Name != Name))
        {
            if (attempts++ > 20)
            {
                Logger.Debug($"SER <-> {Name} bind failed. {Name} specific methods will NOT be loaded.");
                yield break;
            }
            
            yield return Timing.WaitForSeconds(0.1f);
        }
    
        Logger.Raw($"SER <-> {Name} bind was successful! {Name} specific methods will be loaded.", ConsoleColor.Green);
        onDetected?.Invoke();
    }
}