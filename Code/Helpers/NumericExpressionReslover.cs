using System.Globalization;
using System.Text;
using NCalc;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.Helpers;

public static class NumericExpressionReslover
{
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
        int tempId = 0;

        foreach (var token in tokens)
        {
            Log.D($"parsing token {token.RawRep} ({token.GetType().AccurateName})");
            switch (token)
            {
                case IValueToken valueToken when valueToken.CanReturn<LiteralValue>(out var get):
                {
                    var tmp = MakeTempName();
                    
                    if (valueToken.IsConstant)
                    {
                        variables[tmp] = get().OnSuccess(s => s.Value, mainErr);
                    }
                    else
                    {
                        variables[tmp] = new(() => get().OnSuccess(s => s.Value, mainErr));
                    }
                    
                    AppendRaw(tmp);
                    continue;
                }
                case ParenthesesToken parentheses:
                {
                    var tmp = MakeTempName();
                    variables[tmp] = new(() =>
                    {
                        if (parentheses.TryGetTokens().HasErrored(out var tokenizeError, out var tokensInParentheses))
                        {
                            return mainErr + tokenizeError;
                        }
                
                        if (CompileExpression(tokensInParentheses).HasErrored(out var conditonError, out var value))
                        {
                            return mainErr + conditonError;
                        }
                        
                        return value.Evaluate();
                    });
                    
                    AppendRaw(tmp);
                    continue;
                }
                default:
                    AppendRaw(token.RawRep);
                    break;
            }
        }
        
        var expression = new Expression(sb.ToString(), EvaluateOptions.None);

        // Now we have the expression string and a variables dictionary.
        return new CompiledExpression(expression, variables);

        string MakeTempName()
        {
            return "value" + (tempId++).ToString(CultureInfo.InvariantCulture);
        }

        void AppendRaw(string s)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(s);
        }
    }

    // --- Evaluate helper that sets typed params and uses EvaluateParameter ---
    private static TryGet<object> Evaluate(string exprString, Dictionary<string, object> variables, Result rs)
    {
        var expression = new Expression(exprString, EvaluateOptions.None)
        {
            // Supply our typed parameter dictionary:
            Parameters = new Dictionary<string, object>(variables, StringComparer.OrdinalIgnoreCase)
        };

        // As a safe fallback, respond to EvaluateParameter requests by resolving from variables
        expression.EvaluateParameter += (name, args) =>
        {
            if (variables.TryGetValue(name, out var val))
            {
                args.Result = val;
                return;
            }

            // if not found, return a helpful error via exception. NCalc will bubble it.
            throw new ArgumentException($"Unknown identifier '{name}' in expression.");
        };

        try
        {
            var result = expression.Evaluate();
            return new TryGet<object>(result, null); // adapt to your TryGet ctor
        }
        catch (Exception ex)
        {
            // Keep error messages friendly and actionable
            return rs + $"{ex.GetType().AccurateName}: {ex.Message}";
        }
    }

    public readonly struct CompiledExpression(
        Expression expression, 
        Dictionary<string, DynamicTryGet<object>> parameters
    )
    {
        public TryGet<object> Evaluate()
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

            expression.Parameters = values;
            
            try
            {
                return expression.Evaluate();
            }
            catch (Exception ex)
            {
                return $"{ex.GetType().AccurateName}: {ex.Message}";
            }
        }
    }
}