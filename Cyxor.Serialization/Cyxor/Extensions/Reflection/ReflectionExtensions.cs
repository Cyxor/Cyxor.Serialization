using System;
using System.Linq;

namespace Cyxor.Extensions
{
    public static partial class ReflectionExtensions
    {
        public static bool IsInterfaceImplemented<T>(this Type type)
            => IsInterfaceImplemented(type, typeof(T));

        public static bool IsInterfaceImplemented(this Type type, Type interfaceType)
            => !interfaceType.IsInterface
                ? false
                : type == interfaceType
                    ? true
                    : type.IsInterface && interfaceType.IsGenericTypeDefinition && type.GetGenericTypeDefinition() == interfaceType
                        ? true
                        : type.GetInterfaces().Any(p => interfaceType.IsGenericTypeDefinition
                            ? p.IsGenericType && p.GetGenericTypeDefinition() == interfaceType
                            : p == interfaceType);
    }
}