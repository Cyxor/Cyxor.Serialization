using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Cyxor.Extensions
{
    static class StreamExtensions
    {
#if NET20 || NET35
        public static void CopyTo(this System.IO.Stream input, System.IO.Stream output)
        {
            int bytesRead;
            var buffer = new byte[8192 * 4];

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                output.Write(buffer, 0, bytesRead);
        }
#endif

#if NET20 || NET35 || NET40 || NETSTANDARD1_0
        public static bool TryGetBuffer(this MemoryStream value, out ArraySegment<byte> arraySegment)
        {
            var buffer = value.GetBuffer();
            arraySegment = new ArraySegment<byte>(buffer, 0, (int)value.Length);
            return true;
        }
#endif

#if NETSTANDARD1_0 || NETSTANDARD1_3
        public static byte[] GetBuffer(this MemoryStream value)
#if NETSTANDARD1_0
            => value.ToArray();
#else
        {
            value.TryGetBuffer(out var arraySegment);
            return arraySegment.Array;
        }
#endif
#endif
    }

    static class ReflectionExtensions
    {
#if NET20 || NET35 || NET40
        public static Type GetTypeInfo(this Type type) => type;

        public static BindingFlags GenericBindingFlags =
            BindingFlags.DeclaredOnly |
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        public static BindingFlags GenericBindingFlagsPublic =
            BindingFlags.DeclaredOnly |
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public;

        public static BindingFlags GenericBindingFlagsNonPublic =
            BindingFlags.DeclaredOnly |
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.NonPublic;

        public static FieldInfo GetAnyDeclaredField(this Type type, string name)
            => type.GetField(name, GenericBindingFlags);

        public static IEnumerable<FieldInfo> GetDeclaredFields(this Type type)
            => type.GetFields(GenericBindingFlags);

        public static PropertyInfo GetAnyDeclaredProperty(this Type type, string name)
            => type.GetProperty(name, GenericBindingFlags);

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type)
            => type.GetProperties(GenericBindingFlags);

        public static IEnumerable<PropertyInfo> GetDeclaredPublicProperties(this Type type)
            => type.GetProperties(GenericBindingFlagsPublic);

        public static MethodInfo GetAnyDeclaredMethod(this Type type, string name)
            => type.GetMethod(name, GenericBindingFlags);

        /// <summary>
        /// Returns an object that represents the specified public method declared by the current type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name">The name of the method.</param>
        /// <returns>An object that represents the specified method, if found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">name is null.</exception>
        public static MethodInfo GetDeclaredMethod(this Type type, string name) => type.GetMethod(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type, string name)
        {
            foreach (var method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
                if (method.Name == name)
                    yield return method;
        }

#if NULLER
        public static MethodInfo? GetStaticMethod(this Type type, string name)
#else
        public static MethodInfo GetStaticMethod(this Type type, string name)
#endif
        {
            foreach (var method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                if (method.Name == name)
                    return method;

            return null;
        }

#if NULLER
        public static TAttribute? GetCustomAttribute<TAttribute>(this Type type, bool inherit = false)
#else
        public static TAttribute GetCustomAttribute<TAttribute>(this Type type, bool inherit = false)
#endif
            where TAttribute : Attribute
            => type.GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault() as TAttribute;

#if NULLER
        public static TAttribute? GetCustomAttribute<TAttribute>(this MemberInfo element, bool inherit = false) where TAttribute : Attribute
#else
        public static TAttribute GetCustomAttribute<TAttribute>(this MemberInfo element, bool inherit = false) where TAttribute : Attribute
#endif
        {
            var attributes = element.GetCustomAttributes(typeof(TAttribute), inherit);

            if (attributes.Length > 0)
                return (TAttribute)attributes[0];

            return default;
        }

#if NULLER
        public static IEnumerable<TAttribute>? GetCustomAttributes<TAttribute>(this Type type, bool inherit = false) where TAttribute : Attribute
#else
        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this Type type, bool inherit = false) where TAttribute : Attribute
#endif
            => type.GetCustomAttributes(typeof(TAttribute), inherit) as IEnumerable<TAttribute>;

#if NULLER
        public static IEnumerable<TAttribute>? GetCustomAttributes<TAttribute>(this MethodInfo methodInfo, bool inherit = false) where TAttribute : Attribute
#else
        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this MethodInfo methodInfo, bool inherit = false) where TAttribute : Attribute
#endif
            => methodInfo.GetCustomAttributes(typeof(TAttribute), inherit) as IEnumerable<TAttribute>;
#else
        public static MethodInfo GetStaticMethod(this Type type, string name)
            => type.GetTypeInfo().DeclaredMethods.Where(p => p.IsStatic && p.Name == name).SingleOrDefault();

        public static FieldInfo GetAnyDeclaredField(this Type type, string name)
            => type.GetTypeInfo().DeclaredFields.Where(p => p.Name == name).SingleOrDefault();

        public static IEnumerable<FieldInfo> GetDeclaredFields(this Type type)
            => type.GetTypeInfo().DeclaredFields;

        public static PropertyInfo GetAnyDeclaredProperty(this Type type, string name)
            => type.GetTypeInfo().DeclaredProperties.Where(p => p.Name == name).SingleOrDefault();

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type)
            => type.GetTypeInfo().DeclaredProperties;

        public static IEnumerable<PropertyInfo> GetDeclaredPublicProperties(this Type type)
            => type.GetTypeInfo().DeclaredProperties.Where(p => p.GetMethod.IsPublic && p.SetMethod.IsPublic);

        public static MethodInfo GetAnyDeclaredMethod(this Type type, string name)
            => type.GetTypeInfo().DeclaredMethods.Where(p => p.Name == name).SingleOrDefault();
#endif

#if NETSTANDARD1_0 || NETSTANDARD1_3
        public static bool IsDefined(this Type type, Type attributeType)
            => IsDefined(type, attributeType, inherit: false);

        public static bool IsDefined(this Type type, Type attributeType, bool inherit)
            => type.GetTypeInfo().IsDefined(attributeType, inherit);
#endif

        public static bool IsInterfaceImplemented<T>(this Type type)
            => IsInterfaceImplemented(type, typeof(T));

        public static bool IsInterfaceImplemented(this Type type, Type interfaceType)
#if NET20 || NET35 || NET40
            => type.GetInterfaces().Any(p => p == interfaceType);
#else
            => type.GetTypeInfo().ImplementedInterfaces.Any(p => p == interfaceType);
#endif
    }
}