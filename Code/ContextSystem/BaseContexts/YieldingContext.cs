using SER.Code.Helpers;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class YieldingContext : Context
{
    public IEnumerator<float> Run()
    {
        var prof = Script.Profile is not null 
            ? new Profile(Script.Profile, $"running YieldingContext {Name}")
            : null;
        
        if (LineNum.HasValue)
        {
            Script.CurrentLine = LineNum.Value;
        }
        
        var enumerator = Execute();
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }

        prof?.Stop();
    }

    protected abstract IEnumerator<float> Execute();
}