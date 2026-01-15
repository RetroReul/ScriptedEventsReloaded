namespace SER.Code.ValueSystem;

public class TypeOfValue
{
    public TypeOfValue(Type[] required)
    {
        Required = required;
    }
    
    public TypeOfValue(Type required)
    {
        Required = [required];
    }

    protected TypeOfValue()
    {
        Required = null;
    }
    
    public Type[]? Required { get; }
    
    public bool AreKnown(out Type[] known) => (known = Required!) is not null;
    
    public bool DefinitelyAllows<T>() where T : Value => DefinitelyAllows(typeof(T));
    
    public bool DefinitelyAllows(Type checkT)
    {
        if (Required is null) return false;
        return Required.All(requiredT => requiredT.IsAssignableFrom(checkT));
    }
}

public class TypeOfValue<T>() : TypeOfValue(typeof(T))
    where T : Value;

public class UnknownTypeOfValue : TypeOfValue;