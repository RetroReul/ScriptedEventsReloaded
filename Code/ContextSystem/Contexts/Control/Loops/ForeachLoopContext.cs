using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Extensions;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class ForeachLoopContext : LoopContext
{
    private readonly Result _mainErr = "Cannot create 'foreach' loop.";
    
    private VariableToken? _itemVariableToken;
    private bool _usedInKeyword = false;
    private Func<Value[]>? _values = null;

    public override string KeywordName => "foreach";
    public override string Description =>
        "Repeats its body for each player in the player variable or a value in a collection variable, " +
        "assigning it its own custom variable.";
    public override string[] Arguments => ["[variable to assign the item]", "in", "[player/collection variable]"];

    public override Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; } = new();

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (_itemVariableToken is null)
        {
            if (token is VariableToken varToken)
            {
                _itemVariableToken = varToken;
                return TryAddTokenRes.Continue();
            }
            
            return TryAddTokenRes.Error(
                $"'foreach' loop expects to have a variable as its first argument, " +
                $"but received '{token.RawRep}'."
            );
        }

        if (!_usedInKeyword)
        {
            if (token.RawRep != "in")
            {
                return TryAddTokenRes.Error(
                    $"Foreach loop expects to have 'in' keyword as its second argument, " +
                    $"but received '{token.RawRep}'."
                );
            }
            
            _usedInKeyword = true;
            return TryAddTokenRes.Continue();
        }

        if (token is not IValueToken valToken)
        {
            goto Error;
        }

        if (valToken.CanReturn<PlayerValue>(out var getPlayer))
        {
            _values = () =>
            {
                if (getPlayer().HasErrored(out var error, out var value))
                {
                    throw new ScriptRuntimeError(error);
                }

                return value.Players.Select(p => new PlayerValue(p)).ToArray();
            };
            
            return TryAddTokenRes.End();
        }

        if (valToken.CanReturn<CollectionValue>(out var getCollection))
        {
            _values = () =>
            {
                if (getCollection().HasErrored(out var error, out var value))
                {
                    throw new ScriptRuntimeError(error);
                }

                return value.CastedValues;
            };

            return TryAddTokenRes.End();
        }

        Error:
        return TryAddTokenRes.Error(
            "'foreach' loop expected to have either a player value or collection value as its third argument, " +
            $"but received '{token.RawRep}'."
        );
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            _itemVariableToken is not null && 
            _values is not null && 
            _usedInKeyword,
            _mainErr + "Missing required arguments.");
    }

    protected override IEnumerator<float> Execute()
    {
        if (_values is null || _itemVariableToken is null) throw new AndrzejFuckedUpException();

        foreach (var value in _values())
        {
            var itemVar = Variable.CreateVariable(_itemVariableToken.Name, value);
            Script.AddVariable(itemVar);
            
            foreach (var coro in Children.Select(child => child.ExecuteBaseContext()))
            {
                while (coro.MoveNext())
                {
                    yield return coro.Current;
                }
                
                if (ExitLoop)
                {
                    break;
                }

                if (SkipThisIteration)
                {
                    SkipThisIteration = false;
                    break;
                }
            }
            
            Script.RemoveVariable(itemVar);
            
            if (ExitLoop)
            {
                break;
            }
        }
    }
}