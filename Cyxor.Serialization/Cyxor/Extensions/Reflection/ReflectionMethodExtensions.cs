using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Cyxor.Extensions
{
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Retrieves a collection that represents all methods defined on the specified <paramref name="type"/>, 
        /// including inherited, non-public, instance, and static methods.
        /// </summary>
        /// <param name="type">The type that contains the methods.</param>
        /// <returns>A collection of methods for the specified <paramref name="type"/>.</returns>
        public static IEnumerable<MethodInfo> GetAllRuntimeMethods(this Type type)
#if NET20 || NET35 || NET40 || NET45
            => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
#else
            => type.GetRuntimeMethods();
#endif

        /// <summary>
        /// Gets a collection of the methods defined by the current <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type that contains the methods.</param>
        /// <returns>A collection of the methods defined by the current <paramref name="type"/>.</returns>
        public static IEnumerable<MethodInfo> GetAllDeclaredMethods(this Type type)
#if NET20 || NET35 || NET40 || NET45
            => type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
#else
            => type.GetTypeInfo().DeclaredMethods;
#endif

        /// <summary>
        /// Retrieves a collection that represents all methods defined on a specified type when using the default parameters, 
        /// including inherited, non-public, instance, and static methods. Tweak the desired parameters to filter the result.
        /// </summary>
        /// <param name="type">The type that contains the methods.</param>
        /// <param name="name">
        /// The name of the methods.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="nameStartsWith">
        /// The beginning of the methods name.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="nameEndsWith">
        /// The end of the methods name.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="nameContains">
        /// A match string in the methods name.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="inheritedMethods">
        /// A value indicating whether to filter by inherited methods.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="publicMethods">
        /// A value indicating whether to filter by public methods.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="privateMethods">
        /// A value indicating whether the to filter by private methods.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="staticMethods">
        /// A value indicating whether the filter by static methods. 
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="genericMethods">
        /// A value indicating whether to filter by generic methods.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="genericMethodsDefinition">
        /// A value indicating whether to filter by generic methods definition.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="returnType">
        /// The return type of the methods.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="attributes">
        /// An <see cref="IEnumerable{T}"/> that contains custom attributes defined by the methods.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="parametersCount">
        /// The parameters count of the methods.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="parameters">
        /// An <see cref="IEnumerable{T}"/> that contains the methods's parameters.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="genericArgumentsCount">
        /// The generic arguments count of the methods.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="genericArguments">
        /// An <see cref="IEnumerable{T}"/> that contains the methods's generic arguments.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <returns>A collection of methods for the specified type filtered by the supplied parameters.</returns>
        /// <remarks>
        /// Since reflection incurs in costly operations, when performance is important we recommend to cache the results when used repetitively.
        /// For some information on the topic see https://dejanstojanovic.net/aspnet/2019/february/making-reflection-in-net-work-faster/.
        /// </remarks>
        /// <see cref="GetMethodInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetFieldInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetFieldsInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetPropertyInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetPropertiesInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        public static IEnumerable<MethodInfo> GetMethodsInfo(this Type type,
            string? name = default,
            string? nameStartsWith = default,
            string? nameEndsWith = default,
            string? nameContains = default,
            bool? inheritedMethods = default,
            bool? publicMethods = default,
            bool? privateMethods = default,
            bool? staticMethods = default,
            bool? genericMethods = default,
            bool? genericMethodsDefinition = default,
            Type? returnType = default,
            IEnumerable<Type>? attributes = default,
            int? parametersCount = default,
            IEnumerable<Type>? parameters = default,
            int? genericArgumentsCount = default,
            IEnumerable<Type>? genericArguments = default)
            => from method in (inheritedMethods ?? true) ? type.GetAllRuntimeMethods() : type.GetAllDeclaredMethods()
               let methodParameters = method.GetParameters()
               let methodGenericArguments = method.GetGenericArguments()
               where method.Name == (name ?? method.Name)
                && (nameStartsWith == default ? true : method.Name.StartsWith(nameStartsWith!, StringComparison.Ordinal))
                && (nameEndsWith == default ? true : method.Name.EndsWith(nameEndsWith!, StringComparison.Ordinal))
                && (nameContains == default ? true : method.Name.Contains(nameContains!, StringComparison.Ordinal))
                && method.IsPublic == (publicMethods ?? method.IsPublic)
                && method.IsPrivate == (privateMethods ?? method.IsPrivate)
                && method.IsStatic == (staticMethods ?? method.IsStatic)
                && method.IsGenericMethod == (genericMethods ?? method.IsGenericMethod)
                && method.IsGenericMethodDefinition == (genericMethodsDefinition ?? method.IsGenericMethodDefinition)
                && method.ReturnType == (returnType ?? method.ReturnType)
                && (attributes == default ? true : attributes.All(p => method.IsDefined(p, inheritedMethods ?? true)))
                && methodParameters.Length == (parametersCount ?? methodParameters.Length)
                && (parameters == default ? true : methodParameters.Select(p => p.ParameterType).SequenceEqual(parameters))
                && methodGenericArguments.Length == (genericArgumentsCount ?? methodGenericArguments.Length)
                && (genericArguments == default ? true : methodGenericArguments.SequenceEqual(genericArguments))
               select method;

        /// <summary>
        /// Retrieves an object that represents a specified method. Tweak the desired parameters to filter the result.
        /// </summary>
        /// <param name="type">The type that contains the method.</param>
        /// <param name="name">
        /// The name of the method.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="nameStartsWith">
        /// The beginning of the method name.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="nameEndsWith">
        /// The end of the method name.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="nameContains">
        /// A match string in the method name.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="isInherited">
        /// A value indicating whether the method is inherited.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="isPublic">
        /// A value indicating whether the method is public.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="isPrivate">
        /// A value indicating whether the method is private.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="isStatic">
        /// A value indicating whether the method is static.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="isGenericMethod">
        /// A value indicating whether the method is a generic method.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="isGenericMethodDefinition">
        /// A value indicating whether the method is a generic method definition.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="returnType">
        /// The return type of the method.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="attributes">
        /// An <see cref="IEnumerable{T}"/> that contains custom attributes defined by the method.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="parametersCount">
        /// The parameters count of the method.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="parameters">
        /// An <see cref="IEnumerable{T}"/> that contains the method's parameters.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="genericArgumentsCount">
        /// The generic arguments's count of the method.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="genericArguments">
        /// An <see cref="IEnumerable{T}"/> that contains the method's generic arguments.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <returns>An object that represents the specified method, or null if the method is not found.</returns>
        /// <exception cref="AmbiguousMatchException">More than one method is found with the specified parameters.</exception>
        /// <remarks>
        /// Since reflection incurs in costly operations, when performance is important we recommend to cache the results when used repetitively.
        /// For some information on the topic see https://dejanstojanovic.net/aspnet/2019/february/making-reflection-in-net-work-faster/.
        /// </remarks>
        /// <see cref="GetMethodsInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetFieldInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetFieldsInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetPropertyInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetPropertiesInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        public static MethodInfo? GetMethodInfo(this Type type,
            string? name = default,
            string? nameStartsWith = default,
            string? nameEndsWith = default,
            string? nameContains = default,
            bool? isInherited = default,
            bool? isPublic = default,
            bool? isPrivate = default,
            bool? isStatic = default,
            bool? isGenericMethod = default,
            bool? isGenericMethodDefinition = default,
            Type? returnType = default,
            IEnumerable<Type>? attributes = default,
            int? parametersCount = default,
            IEnumerable<Type>? parameters = default,
            int? genericArgumentsCount = default,
            IEnumerable<Type>? genericArguments = default)
        {
            var methods = GetMethodsInfo(type, name, nameStartsWith, nameEndsWith, nameContains, isInherited, 
                isPublic, isPrivate, isStatic, isGenericMethod, isGenericMethodDefinition, returnType, attributes,
                parametersCount, parameters, genericArgumentsCount, genericArguments);

            if (methods.Count() > 1)
                throw new AmbiguousMatchException("More than one method is found with the specified parameters.");

            return methods.SingleOrDefault();
        }
    }
}