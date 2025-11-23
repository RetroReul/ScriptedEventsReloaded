// this gives us the `init` keyword for properties
// if you remove this, the code wont compile
// i still have no clue why is this a thing

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    // Required for init-only properties support in C# 9+ on .NET Framework
    public sealed class IsExternalInit {}
}