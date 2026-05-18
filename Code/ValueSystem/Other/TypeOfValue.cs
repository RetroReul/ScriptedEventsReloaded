namespace SER.Code.ValueSystem.Other;

/// <summary>
/// Represents the type of a value in the value system.
/// </summary>
public abstract class TypeOfValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeOfValue"/> class with a set of required types.
    /// </summary>
    /// <param name="required">The array of required types.</param>
    protected TypeOfValue(Type[] required)
    {
        Required = required;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeOfValue"/> class with a single required type.
    /// </summary>
    /// <param name="required">The required type, or null if unknown.</param>
    protected TypeOfValue(Type? required)
    {
        if (required is null) Required = null;
        else Required = [required];
    }
    
    /// <summary>
    /// Gets the array of types required by this <see cref="TypeOfValue"/>. 
    /// Returns <see langword="null"/> if the type is unknown.
    /// </summary>
    public Type[]? Required { get; }
    
    /// <summary>
    /// Checks if the required types are known and returns them if they are.
    /// </summary>
    /// <param name="known">The array of known types.</param>
    /// <returns><see langword="true"/> if the types are known; otherwise, <see langword="false"/>.</returns>
    public bool AreKnown(out Type[] known) => (known = Required!) is not null;

    /// <summary>
    /// Returns a string that represents the current type.
    /// </summary>
    /// <returns>A string representation of the type.</returns>
    public abstract override string ToString();
    
    /// <summary>
    /// Defines an implicit conversion from <see cref="Type"/> to <see cref="TypeOfValue"/>.
    /// </summary>
    /// <param name="type">The type to convert.</param>
    public static implicit operator TypeOfValue(Type type) => new SingleTypeOfValue(type);
    
    /// <summary>
    /// Combines two <see cref="TypeOfValue"/> instances using a logical OR operation.
    /// </summary>
    /// <param name="left">The left <see cref="TypeOfValue"/>.</param>
    /// <param name="right">The right <see cref="TypeOfValue"/>.</param>
    /// <returns>
    /// A new <see cref="TypeOfValue"/> representing either of the input types. 
    /// If either input is unknown, the result is <see cref="UnknownTypeOfValue"/>.
    /// </returns>
    public static TypeOfValue operator |(TypeOfValue left, TypeOfValue right)
    {
        if (left.Required is null || right.Required is null) return new UnknownTypeOfValue();
        
        var types = new[] {left, right}
            .SelectMany(t => t.Required)
            .Distinct()
            .ToArray();

        if (types.Length == 1) return new SingleTypeOfValue(types[0]);
        
        return new TypesOfValue(types);
    }
}

/// <summary>
/// Represents a <see cref="TypeOfValue"/> that can be one of several possible types.
/// </summary>
/// <param name="types">The possible types.</param>
public class TypesOfValue(params Type[] types) : TypeOfValue(types)
{
    private readonly Type[] _types = types;
    
    /// <inheritdoc />
    public override string ToString() => 
        string.Join(" or ", _types.Select(Value.GetFriendlyName));
}

/// <summary>
/// Represents an unknown <see cref="TypeOfValue"/>.
/// </summary>
public class UnknownTypeOfValue() : TypeOfValue((Type?)null)
{
    /// <inheritdoc />
    public override string ToString() => "unknown value";
}

/// <summary>
/// Represents a <see cref="TypeOfValue"/> that is a single specific type.
/// </summary>
/// <param name="type">The specific type.</param>
public class SingleTypeOfValue(Type type) : TypeOfValue(type)
{
    /// <summary>
    /// Gets the specific <see cref="System.Type"/> represented by this instance.
    /// </summary>
    public readonly Type Type = type;

    private static Type Unwrap(Type type) =>
        (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Invalidable<>))
            ? type.GetGenericArguments()[0]
            : type;

    /// <summary>
    /// Checks if this type is the same as another <see cref="SingleTypeOfValue"/>.
    /// </summary>
    /// <param name="otherType">The other type to compare with.</param>
    /// <returns><see langword="true"/> if the types are the same; otherwise, <see langword="false"/>.</returns>
    public bool Is(SingleTypeOfValue otherType) => otherType.Type == Type;
    
    /// <summary>
    /// Checks if this type is the same as the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to compare with.</typeparam>
    /// <returns><see langword="true"/> if the types are the same; otherwise, <see langword="false"/>.</returns>
    public bool Is<T>() where T : Value => Is(typeof(T));
    
    /// <summary>
    /// Checks if this type is the same or a derived type of <paramref name="otherType"/>, 
    /// considering <see cref="Invalidable{T}"/> unwrapping.
    /// </summary>
    /// <param name="otherType">The other type to compare with.</param>
    /// <returns><see langword="true"/> if this type is the same or higher; otherwise, <see langword="false"/>.</returns>
    public bool IsSameOrHigherThan(SingleTypeOfValue otherType)
    {
        if (otherType.Type.IsAssignableFrom(Type)) return true;
        return Unwrap(otherType.Type).IsAssignableFrom(Unwrap(Type));
    }
    
    /// <summary>
    /// Checks if this type is the same or a derived type of <typeparamref name="T"/>, 
    /// considering <see cref="Invalidable{T}"/> unwrapping.
    /// </summary>
    /// <typeparam name="T">The type to compare with.</typeparam>
    /// <returns><see langword="true"/> if this type is the same or higher; otherwise, <see langword="false"/>.</returns>
    public bool IsSameOrHigherThan<T>() where T : Value => IsSameOrHigherThan(typeof(T));
    
    /// <summary>
    /// Checks if this type can hold a value of <paramref name="otherType"/>, 
    /// considering <see cref="Invalidable{T}"/> unwrapping.
    /// </summary>
    /// <param name="otherType">The other type to check.</param>
    /// <returns><see langword="true"/> if this type can hold the other type; otherwise, <see langword="false"/>.</returns>
    public bool CanHold(SingleTypeOfValue otherType)
    {
        if (Type.IsAssignableFrom(otherType.Type)) return true;
        return Unwrap(Type).IsAssignableFrom(Unwrap(otherType.Type));
    }
    
    /// <summary>
    /// Checks if this type can hold a value of type <typeparamref name="T"/>, 
    /// considering <see cref="Invalidable{T}"/> unwrapping.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns><see langword="true"/> if this type can hold the other type; otherwise, <see langword="false"/>.</returns>
    public bool CanHold<T>() where T : Value => CanHold(typeof(T));
    
    /// <inheritdoc />
    public override string ToString() => Value.GetFriendlyName(Type);
    
    /// <summary>
    /// Defines an implicit conversion from <see cref="Type"/> to <see cref="SingleTypeOfValue"/>.
    /// </summary>
    /// <param name="type">The type to convert.</param>
    public static implicit operator SingleTypeOfValue(Type type) => new(type);
}

/// <summary>
/// Represents a <see cref="SingleTypeOfValue"/> for a specific value type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
public class TypeOfValue<T>() : SingleTypeOfValue(typeof(T))
    where T : Value;