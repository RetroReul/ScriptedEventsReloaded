using SER.Code.ContextSystem.BaseContexts;
using SER.Code.Helpers.ResultSystem;

namespace SER.Code.ContextSystem.Interfaces;

/// <summary>
/// Requests the previous statement context to be provided
/// </summary>
public interface IRequirePreviousStatementContext
{
    public Result AcceptStatement(StatementContext context);
}