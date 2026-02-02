namespace SER.Code.Helpers.Exceptions;

public class ScriptCompileError(string error) : SystemException(error);