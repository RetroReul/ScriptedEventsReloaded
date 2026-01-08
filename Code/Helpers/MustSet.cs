namespace SER.Code.Helpers;

public struct MustSet<T>
{
    private readonly bool _set;

    public T Value
    {
        get
        {
            if (!_set)
                throw new InvalidOperationException($"Attempted to get {typeof(T).Name} before it was set.");
            return field;
        }
        init
        {
            field = value;
            _set = true;
        }
    }

    public static implicit operator T(MustSet<T> wrapper) => wrapper.Value;
    public static implicit operator MustSet<T>(T value) => new() { Value = value };
}