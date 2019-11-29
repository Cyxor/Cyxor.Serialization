using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Cyxor.Extensions
{
    public static partial class ReflectionExtensions
    {
#if NET20 || NET35 || NET40
        public static Type GetTypeInfo(this Type type) => type;
#endif

        public static bool IsInterfaceImplemented<T>(this Type type)
            => IsInterfaceImplemented(type, typeof(T));

        public static bool IsInterfaceImplemented(this Type type, Type interfaceType)
#if NET20 || NET35 || NET40
            => type.GetInterfaces().Any(p => p == interfaceType);
#else
            => type.GetTypeInfo().ImplementedInterfaces.Any(p => p == interfaceType);
#endif

#if NETSTANDARD1_0 || NETSTANDARD1_3
        //public static FieldInfo[] GetFields(this Type type)
        //    => type.GetTypeInfo().DeclaredFields.ToArray();

        //public static PropertyInfo[] GetProperties(this Type type)
        //    => type.GetTypeInfo().DeclaredProperties.ToArray();

        // TODO:
        //public static bool IsDefined(this Type type, Type attributeType)
        //    => IsDefined(type, attributeType, inherit: false);

        //public static bool IsDefined(this Type type, Type attributeType, bool inherit)
        //    => type.GetTypeInfo().IsDefined(attributeType, inherit);

        public static Type[] GetGenericArguments(this Type type)
            => type.GetTypeInfo().GenericTypeArguments;
#endif
    }
}