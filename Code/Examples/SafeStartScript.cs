using JetBrains.Annotations;

namespace SER.Code.Examples;

[UsedImplicitly]
public class SafeStartScript : Example
{
    public override string Name => "safeStart";

    public override string Content =>
        """
        !-- OnEvent RoundStarted
        
        if {AmountOf @all} isnt 1
            stop
        end
        
        Print "SER enabled roundlock!"
        Broadcast @all 10s "<color=green>SER enabled roundlock!"
        RoundLock true
        """;
}