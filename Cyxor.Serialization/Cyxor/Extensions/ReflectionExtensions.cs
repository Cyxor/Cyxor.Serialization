using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Cyxor.Extensions
{
    public static class ReflectionExtensions
    {
#if NET20 || NET35 || NET40
        public static Type GetTypeInfo(this Type type) => type;
#endif

        public static IEnumerable<MethodInfo> GetAllMethods(this Type type)
#if NET20 || NET35 || NET40 || NET45
            => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
#else
            => type.GetRuntimeMethods();
#endif

        public static IEnumerable<MethodInfo> GetAllDeclaredMethods(this Type type)
#if NET20 || NET35 || NET40 || NET45
            => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
#else
            => type.GetTypeInfo().DeclaredMethods.Where(p => p.IsPrivate || p.IsPublic || p.IsStatic || p.IsFamily || p.IsFamilyOrAssembly || p.IsFamilyAndAssembly);
#endif

#if NETSTANDARD1_0 || NETSTANDARD1_3
        public static MethodInfo? GetGetMethod(this PropertyInfo type)
            => type.GetMethod;

        public static MethodInfo? GetSetMethod(this PropertyInfo type)
            => type.SetMethod;

        public static FieldInfo[] GetFields(this Type type)
            => type.GetTypeInfo().DeclaredFields.ToArray();

        public static PropertyInfo[] GetProperties(this Type type)
            => type.GetTypeInfo().DeclaredProperties.ToArray();

        // TODO:
        //public static bool IsDefined(this Type type, Type attributeType)
        //    => IsDefined(type, attributeType, inherit: false);

        //public static bool IsDefined(this Type type, Type attributeType, bool inherit)
        //    => type.GetTypeInfo().IsDefined(attributeType, inherit);

        public static Type[] GetGenericArguments(this Type type)
            => type.GetTypeInfo().GenericTypeArguments;
#endif

        public static IEnumerable<MethodInfo> GetMethodsInfo(this Type type,
            string? name = default,
            string? nameStartsWith = default,
            bool? isPublic = default,
            bool? isStatic = default,
            bool? isInherited = default,
            int? parametersCount = default,
            bool? isGenericMethod = default,
            bool? isGenericMethodDefinition = default,
            int? genericArgumentsCount = default)
            => ((isInherited ?? true) ? type.GetAllMethods() : type.GetAllDeclaredMethods()).Where(p =>
                p.Name == (name ?? p.Name)
                && (nameStartsWith == default ? true : p.Name.StartsWith(nameStartsWith!, StringComparison.Ordinal))
                && p.IsPublic == (isPublic ?? p.IsPublic)
                && p.IsPrivate == (isPublic == default ? p.IsPrivate : !isPublic)
                && p.IsStatic == (isStatic ?? p.IsStatic)
                && p.GetParameters().Length == (parametersCount ?? p.GetParameters().Length)
                && p.IsGenericMethod == (isGenericMethod ?? p.IsGenericMethod)
                && p.IsGenericMethodDefinition == (isGenericMethodDefinition ?? p.IsGenericMethodDefinition)
                && p.GetGenericArguments().Length == (genericArgumentsCount ?? p.GetGenericArguments().Length));

        public static MethodInfo? GetMethodInfo(this Type type,
            string? name = default,
            string? nameStartsWith = default,
            bool? isPublic = default,
            bool? isStatic = default,
            bool? isInherited = default,
            int? parametersCount = default,
            bool? isGenericMethod = default,
            bool? isGenericMethodDefinition = default,
            int? genericArgumentsCount = default)
            => GetMethodsInfo(type, name, nameStartsWith, isPublic, isStatic, isInherited, parametersCount, isGenericMethod, isGenericMethodDefinition,
                genericArgumentsCount)
                .SingleOrDefault();

        public static IEnumerable<FieldInfo> GetFields(this Type type,
            string? name = default,
            bool? isPublic = default,
            bool? isStatic = default,
            bool? isInitOnly = default,
            bool? isLiteral = default)
            => type.GetFields().Where(p =>
                p.Name == (name ?? p.Name)
                && p.IsPublic == (isPublic ?? p.IsPublic)
                && p.IsPrivate == (isPublic == default ? p.IsPrivate : !isPublic)
                && p.IsStatic == (isStatic ?? p.IsStatic)
                && p.IsInitOnly == (isInitOnly ?? p.IsInitOnly)
                && p.IsLiteral == (isLiteral ?? p.IsLiteral));

        public static FieldInfo? GetField(this Type type,
            string? name = default,
            bool? isPublic = default,
            bool? isStatic = default,
            bool? isInitOnly = default,
            bool? isLiteral = default)
            => GetFields(type, name, isPublic, isStatic, isInitOnly, isLiteral).SingleOrDefault();

        public static IEnumerable<PropertyInfo> GetProperties(this Type type,
            string? name = default,
            bool? isPublic = default,
            bool? isStatic = default,
            bool? canRead = default,
            bool? canWrite = default)
            => type.GetProperties().Where(p =>
                p.Name == (name ?? p.Name)
                && (isPublic == default ? true
                    : isPublic == true
                        ? (p.GetGetMethod()?.IsPublic ?? false) || (p.GetSetMethod()?.IsPublic ?? false)
                        : !(p.GetGetMethod()?.IsPublic ?? false) && !(p.GetSetMethod()?.IsPublic ?? false))
                && (isStatic == default ? true
                    : isStatic == true
                        ? (p.GetGetMethod()?.IsStatic ?? false) || (p.GetSetMethod()?.IsStatic ?? false)
                        : !(p.GetGetMethod()?.IsStatic ?? false) && !(p.GetSetMethod()?.IsStatic ?? false))
                && p.CanRead == (canRead ?? p.CanRead)
                && p.CanWrite == (canWrite ?? p.CanWrite));

        public static PropertyInfo? GetProperty(this Type type,
            string? name = default,
            bool? isPublic = default,
            bool? isStatic = default,
            bool? canRead = default,
            bool? canWrite = default)
            => GetProperties(type, name, isPublic, isStatic, canRead, canWrite).SingleOrDefault();

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