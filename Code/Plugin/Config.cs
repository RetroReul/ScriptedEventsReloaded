namespace SER.Code.Plugin;

public class Config
{
    public bool IsEnabled { get; set; } = true;
    
    public bool SerMethodsAsCommands { get; set; } = false;
    
    public bool SendHelpMessageOnServerInitialization { get; set; } = true;
}