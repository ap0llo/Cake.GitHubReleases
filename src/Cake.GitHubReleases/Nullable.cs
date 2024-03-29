﻿// Defines additional attributes for C# 8's nullable reference types
// These types are built-in to .NET Core 3 and later, but not available on .NET Standard 2.0
// To avoid having to #ifdef every usage of the attributes, they are defined here as internal for .NET Standard 2.0
#if NETSTANDARD2_0

namespace System.Diagnostics.CodeAnalysis
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
    internal sealed class AllowNullAttribute : Attribute
    { }

    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        public bool ReturnValue { get; }


        public NotNullWhenAttribute(bool returnValue)
        {
            ReturnValue = returnValue;
        }
    }
}

#endif
