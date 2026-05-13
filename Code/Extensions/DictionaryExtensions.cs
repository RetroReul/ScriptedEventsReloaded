namespace SER.Code.Extensions;

public static class DictionaryExtensions
{
    public static void AddOrInitListWithKey<TKey, TValueColl, TValue>(
        this Dictionary<TKey, TValueColl> dictionary, 
        TKey key, 
        TValue value
    ) where TValueColl : ICollection<TValue>, new()
    {
        if (dictionary.TryGetValue(key, out var list))
        {
            list.Add(value);
        }
        else
        {
            dictionary[key] = [value];
        }
    }
    
    public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
    {
        key = tuple.Key;
        value = tuple.Value;
    }
}