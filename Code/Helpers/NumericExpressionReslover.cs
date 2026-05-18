using System.Globalization;
using System.Text;
using NCalc;
using NCalc.Domain;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ValueTokens;
using SER.Code.ValueSystem;

namespace SER.Code.Helpers;

public static class NumericExpressionReslover
{
    public static Result IsValidForExpression(BaseToken token)
    {
        if (ParseToken(token, new(), new(), "Expression is invalid.", 0)
            .HasErrored(out var err))
        {
            return err;
        }
        
        return true;
    }
    
    public static TryGet<CompiledExpression> CompileExpression(string expression, Script scr)
    {
        Result mainErr = $"Condition '{expression}' is invalid.";
        
        if (Tokenizer.TokenizeLine(expression, scr, null)
            .HasErrored(out var err, out var tokens))
        {
            return mainErr + err;
        }
        
        return CompileExpression(tokens.ToArray());
    }

    public static TryGet<CompiledExpression> CompileExpression(BaseToken[] tokens)
    {
        var initial = tokens.Select(t => t.RawRep).JoinStrings(" ");
        Result mainErr = $"Expression '{initial}' is invalid.";

        var variables = new Dictionary<string, DynamicTryGet<object>>(StringComparer.OrdinalIgnoreCase);
        var sb = new StringBuilder();
        uint tempId = 0;

        foreach (var token in tokens)
        {
            tempId++;
            Log.D($"parsing token {token.RawRep} ({token.GetType().AccurateName})");
            
            if (ParseToken(token, variables, sb, mainErr, tempId).HasErrored(out var err))
            {
                return err;
            }
        }

        LogicalExpression expression;
        try
        {
            expression = Expression.Compile(sb.ToString(), false);
        }
        catch (EvaluationException err)
        {
            return $"Cannot compile expression '{initial}': {err.Message}";
        }
        
        // Now we have the expression string and a variables dictionary.
        return new CompiledExpression(
            expression, 
            variables
        );
    }

    public readonly struct CompiledExpression(
        LogicalExpression expression, 
        Dictionary<string, DynamicTryGet<object>> parameters
    )
    {
        public object? Evaluate()
        {
            Dictionary<string, object> values = [];
            foreach (var (key, dtg) in parameters)
            {
                if (dtg.Invoke().HasErrored(out var err, out var value))
                {
                    return err;
                }

                values[key] = value;
            }

            var visitor = new EvaluationVisitor(EvaluateOptions.None)
            {
                Parameters = values
            };
            
            try
            {
                expression.Accept(visitor);
            }   
            catch (Exception)
            {
                // this means that the expression was syntatically valid, but the values were not.
                // in that case, throwing an error is not ideal.
                // we will leave this up to the individual implementations.
                return null;
            }
            
            return visitor.Result;
        }
    }

    private static Result ParseToken(
        BaseToken token,
        Dictionary<string, DynamicTryGet<object>> variables,
        StringBuilder sb, 
        Result mainErr,
        uint tempId)
    {
        switch (token)
        {
            case ValueToken valueToken:
            {
                valueToken.CapableOf<ReferenceValue>(out var referencefGet);
                valueToken.CapableOf<LiteralValue>(out var literalGet);

                if (referencefGet is null && literalGet is null)
                {
                    goto default;
                }
                
                var tmp = MakeTempName();

                if (referencefGet is not null && literalGet is not null)
                {
                    variables[tmp] = new(() =>
                    {
                        var literalVal = literalGet();
                        if (literalVal.WasSuccessful(out var literalValue))
                        {
                            return TryGet<object>.Success(literalValue.UnderlyingValue);
                        }
                        
                        var refVal = referencefGet();
                        if (refVal.WasSuccessful(out var refValue))
                        {
                            return TryGet<object>.Success(refValue.ToString());
                        }
                        
                        return TryGet<object>.Error($"{valueToken} did not return a reference value nor a literal value.");
                    });
                }
                else if (referencefGet is not null)
                {
                    variables[tmp] = new(() => referencefGet.Invoke().OnSuccess(v => v.ToString()));
                }
                else if (literalGet is not null)
                {
                    variables[tmp] = new(() => literalGet.Invoke().OnSuccess(v => v.UnderlyingValue));
                }
                
                AppendRaw(tmp);
                return true;
            }
            case
            {
                RawRep: 
                    "==" or "!=" or ">" or ">=" or "<" or "<=" 
                    or "+" or "-" or "*" or "/" or "%" 
                    or "&&" or "||"
            }:
            {
                AppendRaw(token.RawRep);
                return true;
            }
            case { RawRep: "=" or "is" }:
            {
                AppendRaw("==");
                return true;
            }
            case { RawRep: "isnt" or "isn't" or "isnot" }:
            {
                AppendRaw("!=");
                return true;
            }
            case { RawRep: "and" }:
            {
                AppendRaw("&&");
                return true;
            }
            case { RawRep: "or" }:
            {
                AppendRaw("||");
                return true;
            }
            default:
            {
                return mainErr + $"{token} cannot be used in an expression. Maybe you made a typo?";
            }
        }
        
        string MakeTempName()
        {
            return "value" + tempId.ToString(CultureInfo.InvariantCulture);
        }

        void AppendRaw(string s)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(s);
        }
    }
}