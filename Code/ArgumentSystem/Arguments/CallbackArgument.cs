using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ArgumentSystem.Arguments;

public class CallbackArgument(string name, params (SingleTypeOfValue type, string name)[] requiredArguments) : Argument(name)
{
    public override string InputDescription => 
        "A name of a function defined above e.g. MyFunction" +
        (requiredArguments.Length > 0 
            ? $". It has to have these arguments: {requiredArguments.Select(x => $"{Value.GetPrefixOfValue(x.type)}{x.name}").JoinStrings(" ")}"
            : string.Empty
        );
    
    [UsedImplicitly]
    public DynamicTryGet<Action<Value[]>> GetConvertSolution(BaseToken token)
    {
        if (token.BestDynamicTextRepr().IsStatic(out var value, out var func))
        {
            return Verify(value);
        }
        
        return new(() => Verify(func()));
    }

    private TryGet<Action<Value[]>> Verify(string name)
    {
        if (!Script.DefinedFunctions.TryGetValue(name, out var func))
        {
            return $"There is no function called '{name}'";
        }

        if (func.ExpectedVariables.Length != requiredArguments.Length)
        {
            return $"The amount of expected variables in the '{name}' function is {func.ExpectedVariables.Length}, " +
                   $"but {requiredArguments.Length} are needed.";
        }

        for (int i = 0; i < requiredArguments.Length; i++)
        {
            var requiredType = requiredArguments[i].type;
            var definedType = func.ExpectedVariables[i].ValueType;

            if (!definedType.CanHold(requiredType))
            {
                return $"Method expects the argument #{i + 1} to be of '{requiredType}', " +
                       $"but '{name}' function defines it to be of '{definedType}'.";
            }
        }

        if (func.LineNum is not { } startLine)
        {
            return $"Cannot find the beginning of the '{name}' function - this should not happen.";
        }


        if (func.EndLine is not { } endLine)
        {
            return $"Cannot find the end of the '{name}' function - this should not happen.";
        }
        
        var funcContent = Script
            .Content
            .Replace("\r", string.Empty)
            .Split('\n')
            .AsSpan((int)startLine, (int)endLine - (int)startLine - 1)
            .ToArray()
            .JoinStrings("\n");
        
        return new(
            args => Script.CreateForCallback(
                $"'{func.FunctionName}' function from '{Script.Name}' script",
                funcContent,
                Script.Executor,
                scr =>
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        var v = Variable.Create(func.ExpectedVariables[i].Name, args[i]);
                        scr.AddLocalVariable(v);
                    }
                }
            ).Run(), 
            null
        );
    }
}