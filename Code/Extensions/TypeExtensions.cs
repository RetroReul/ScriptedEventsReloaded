using System.Text;

namespace SER.Code.Extensions;

public static class TypeExtensions
{
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
            return Activator.CreateInstance(type);
        }
        
        public T CreateInstance<T>()
        {
            return (T) Activator.CreateInstance(type);
        }
        
        public string FriendlyTypeName(bool lowerCase = false)
        {
            return type.Name.Spaceify(lowerCase);
        }
    }
}