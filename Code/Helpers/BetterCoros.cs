using MEC;
using SER.Code.Helpers.Exceptions;
using SER.Code.ScriptSystem;
using SER.Code.Helpers.Extensions;

namespace SER.Code.Helpers;

public static class BetterCoros
{
    public static CoroutineHandle Run(
        this IEnumerator<float> coro, 
        Script scr, 
        Action<Exception>? onException = null,
        Action? onFinish = null
    )
    {
        return Timing.RunCoroutine(Wrapper(coro, scr, onException, onFinish));
    }

    public static void Kill(this CoroutineHandle coro)
    {
        Timing.KillCoroutines(coro);
    }

    private static IEnumerator<float> Wrapper(
        IEnumerator<float> routine, 
        Script scr, 
        Action<Exception>? onException = null,
        Action? onFinish = null
    )
    {
        while (true)
        {
            try
            {
                if (!routine.MoveNext()) goto End;
            }
            catch (ScriptCompileError compErr)
            {
                onException?.Invoke(compErr);
                scr.Error(compErr.Message);
            }
            catch (ScriptRuntimeError runErr)
            {
                onException?.Invoke(runErr);
                scr.Error($"SER was not able to predict this error before running the script: " +
                          $"{runErr.Message ?? "ERROR UNSPECIFIED"}");
                goto End;
            }
            catch (DeveloperFuckedUpException devErr)
            {
                onException?.Invoke(devErr);
                scr.Error(devErr.Message + "\n" + devErr.StackTrace);
                goto End;
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
                scr.Error($"Coroutine failed with {ex.GetType().AccurateName}: {ex.Message}\n{ex.StackTrace}");
                goto End;
            }

            yield return routine.Current;
        }

        End:
        onFinish?.Invoke();
    }
}