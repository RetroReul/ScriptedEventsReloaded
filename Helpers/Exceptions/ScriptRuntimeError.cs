namespace SER.Helpers.Exceptions;

public class ScriptRuntimeError(string error) : SystemException(error);