namespace SER.Code.Helpers.Exceptions;

public class ScriptRuntimeError(string error) : SystemException(error);