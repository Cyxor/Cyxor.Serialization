using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Cyxor.Extensions
{
    static class ReflectionExtensions
    {
        public static Func<MethodInfo, bool> GetMethodsPredicate(string? name = default, bool? isPublic = default,
            bool? isStatic = default, int? parametersCount = default, bool? isGenericMethod = default,
            bool? isGenericMethodDefinition = default, int? genericArgumentsCount = default)
            => p
                => p.Name == (name ?? p.Name)
                && p.IsPublic == (isPublic ?? p.IsPublic)
                && p.IsPrivate == (isPublic == default ? p.IsPrivate : !isPublic)
                && p.IsStatic == (isStatic ?? p.IsStatic)
                && p.GetParameters().Length == (parametersCount ?? p.GetParameters().Length)
                && p.IsGenericMethod == (isGenericMethod ?? p.IsGenericMethod)
                && p.IsGenericMethodDefinition == (isGenericMethodDefinition ?? p.IsGenericMethodDefinition)
                && p.GetGenericArguments().Length == (genericArgumentsCount ?? p.GetGenericArguments().Length);

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

        public static IEnumerable<MethodInfo> GetMethods(this Type type, string? name = default, bool? isPublic = default,
            bool? isStatic = default, int? parametersCount = default, bool? isGenericMethod = default,
            bool? isGenericMethodDefinition = default, int? genericArgumentsCount = default)
            => type.GetMethods().Where(
                GetMethodsPredicate(name, isPublic, isStatic, parametersCount, isGenericMethod, isGenericMethodDefinition, genericArgumentsCount));

        public static MethodInfo GetMethod(this Type type, string? name = default, bool? isPublic = default,
            bool? isStatic = default, int? parametersCount = default, bool? isGenericMethod = default,
            bool? isGenericMethodDefinition = default, int? genericArgumentsCount = default)
            => GetMethods(type, name, isPublic, isStatic, parametersCount, isGenericMethod, isGenericMethodDefinition,
                genericArgumentsCount)
                .SingleOrDefault();

        public static TAttribute? GetCustomAttribute<TAttribute>(this Type type, bool inherit = false)
            where TAttribute : Attribute
            => type.GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault() as TAttribute;

        public static TAttribute? GetCustomAttribute<TAttribute>(this MemberInfo element, bool inherit = false) where TAttribute : Attribute
            => element.GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault() as TAttribute;

        public static IEnumerable<TAttribute>? GetCustomAttributes<TAttribute>(this Type type, bool inherit = false) where TAttribute : Attribute
            => type.GetCustomAttributes(typeof(TAttribute), inherit) as IEnumerable<TAttribute>;

        public static IEnumerable<TAttribute>? GetCustomAttributes<TAttribute>(this MethodInfo methodInfo, bool inherit = false) where TAttribute : Attribute
            => methodInfo.GetCustomAttributes(typeof(TAttribute), inherit) as IEnumerable<TAttribute>;
#else
        public static FieldInfo GetAnyDeclaredField(this Type type, string name)
            => type.GetTypeInfo().DeclaredFields.Where(p => p.Name == name).SingleOrDefault();

        public static IEnumerable<FieldInfo> GetDeclaredFields(this Type type)
            => type.GetTypeInfo().DeclaredFields;

        public static PropertyInfo GetAnyDeclaredProperty(this Type type, string name)
            => type.GetTypeInfo().DeclaredProperties.Where(p => p.Name == name).SingleOrDefault();

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type)
            => type.GetTypeInfo().DeclaredProperties;

        public static IEnumerable<PropertyInfo> GetDeclaredPublicProperties(this Type type)
            => type.GetTypeInfo().DeclaredProperties.Where(p => (p.GetMethod?.IsPublic ?? true) && (p.SetMethod?.IsPublic ?? true));

        public static IEnumerable<MethodInfo> GetMethods(this Type type, string? name = default, bool? isPublic = default,
            bool? isStatic = default, int? parametersCount = default, bool? isGenericMethod = default,
            bool? isGenericMethodDefinition = default, int? genericArgumentsCount = default)
            => type.GetTypeInfo().DeclaredMethods.Where(
                GetMethodsPredicate(name, isPublic, isStatic, parametersCount, isGenericMethod, isGenericMethodDefinition, genericArgumentsCount));

        public static MethodInfo GetMethod(this Type type, string? name = default, bool? isPublic = default,
            bool? isStatic = default, int? parametersCount = default, bool? isGenericMethod = default,
            bool? isGenericMethodDefinition = default, int? genericArgumentsCount = default)
            => GetMethods(type, name, isPublic, isStatic, parametersCount, isGenericMethod, isGenericMethodDefinition,
                genericArgumentsCount)
                .SingleOrDefault();
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