using System.Linq.Expressions;
using System.Text;

namespace SER.Code.Extensions;

public static class TypeExtensions
{
    private static readonly Dictionary<Type, Func<object>> ConstructorCache = new();

    extension(Type type)
    {
        public string AccurateName
        {
            get
            {
                if (!type.IsGenericType)
                    return type.Name;
                
                var sb = new StringBuilder();
                string name = type.Name;
                int index = name.IndexOf('`');
                if (index > 0)
                    name = name[..index];

                sb.Append(name);
                sb.Append('<');
                var args = type.GetGenericArguments();
                for (int i = 0; i < args.Length; i++)
                {
                    if (i > 0)
                        sb.Append(", ");
                    sb.Append(args[i].AccurateName); // recursion for nested generics
                }
                sb.Append('>');
                return sb.ToString();
            }
        }
        
        public object CreateInstance()
        {
            if (ConstructorCache.TryGetValue(type, out var ctor))
                return ctor();

            var newExp = Expression.New(type);
            var lambda = Expression.Lambda<Func<object>>(Expression.Convert(newExp, typeof(object)));
            ctor = lambda.Compile();
            ConstructorCache[type] = ctor;
            return ctor();
        }
        
        public T CreateInstance<T>()
        {
            return (T) type.CreateInstance();
        }
        
        public string FriendlyTypeName(bool lowerCase = false)
        {
            return type.Name.Spaceify(lowerCase);
        }
    }
}