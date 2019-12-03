using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Cyxor.Extensions
{
    public static partial class ReflectionExtensions
    {
#if NETSTANDARD1_0 || NETSTANDARD1_3
        /// <summary>
        /// Returns the public or non-public get accessor for this property.
        /// </summary>
        /// <param name="propertyInfo">The property object that contains the get accessor.</param>
        /// <param name="nonPublic">
        /// Indicates whether a non-public get accessor should be returned, <see langword="true"/> if a non-public accessor is to be returned; 
        /// otherwise, <see langword="false"/>.
        /// </param>
        /// <returns>
        /// A MethodInfo object representing the get accessor for this property, if nonPublic is true.
        /// Returns null if nonPublic is false and the get accessor is non-public, or if nonPublic is true but no get accessors exist.
        /// </returns>
        public static MethodInfo? GetGetMethod(this PropertyInfo propertyInfo, bool nonPublic)
            => !nonPublic && propertyInfo.GetMethod.IsPrivate ? null : propertyInfo.GetMethod;

        /// <summary>
        /// Returns the public or non-public set accessor for this property.
        /// </summary>
        /// <param name="propertyInfo">The property object that contains the set accessor.</param>
        /// <param name="nonPublic">
        /// Indicates whether a non-public set accessor should be returned, <see langword="true"/> if a non-public accessor is to be returned; 
        /// otherwise, <see langword="false"/>.
        /// </param>
        /// <returns>
        /// A MethodInfo object representing the set accessor for this property, if nonPublic is true.
        /// Returns null if nonPublic is false and the set accessor is non-public, or if nonPublic is true but no set accessors exist.
        /// </returns>
        public static MethodInfo? GetSetMethod(this PropertyInfo propertyInfo, bool nonPublic)
            => !nonPublic && propertyInfo.SetMethod.IsPrivate ? null : propertyInfo.SetMethod;
#endif

        /// <summary>
        /// Retrieves a collection that represents all the properties defined on the specified <paramref name="type"/>, 
        /// including inherited, non-public, instance, and static properties.
        /// </summary>
        /// <param name="type">The type that contains the properties.</param>
        /// <returns>A collection of properties for the specified <paramref name="type"/>.</returns>
        public static IEnumerable<PropertyInfo> GetAllRuntimeProperties(this Type type)
#if NET20 || NET35 || NET40 || NET45
            => type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
#else
            => type.GetRuntimeProperties();
#endif

        /// <summary>
        /// Gets a collection of the properties defined by the current <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type that contains the properties.</param>
        /// <returns>A collection of the properties defined by the current <paramref name="type"/>.</returns>
        public static IEnumerable<PropertyInfo> GetAllDeclaredProperties(this Type type)
#if NET20 || NET35 || NET40 || NET45
            => type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
#else
            => type.GetTypeInfo().DeclaredProperties;
#endif

        /// <summary>
        /// Retrieves a collection that represents all properties defined on a specified type when using the default parameters, 
        /// including inherited, non-public, instance, and static properties. Tweak the desired parameters to filter the result.
        /// </summary>
        /// <param name="type">The type that contains the properties.</param>
        /// <param name="name">
        /// The name of the properties.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="nameStartsWith">
        /// The beginning of the properties name.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="nameEndsWith">
        /// The end of the properties name.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="nameContains">
        /// A match string in the properties name.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="inheritedProperties">
        /// A value indicating whether to filter by inherited properties.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="publicGetAccessor">
        /// A value indicating whether to filter by properties with a public get accessor.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="privateGetAccessor">
        /// A value indicating whether to filter by properties with a private get accessor.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="staticGetAccessor">
        /// A value indicating whether to filter by properties with a static get accessor.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="genericPropertyType">
        /// A value indicating whether to filter by generic PropertyType.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="genericPropertyTypeDefinition">
        /// A value indicating whether to filter by generic PropertyType definition.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="canReadProperties">
        /// A value indicating whether to filter by can-read properties.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="canWriteProperties">
        /// A value indicating whether to filter by can-write properties.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="propertyType">
        /// The type of the properties.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="attributes">
        /// An <see cref="IEnumerable{T}"/> that contains custom attributes defined by the properties.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="genericArgumentsCount">
        /// The generic arguments count of the properties.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="genericArguments">
        /// An <see cref="IEnumerable{T}"/> that contains the properties's generic arguments.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <returns>A collection of properties for the specified type filtered by the supplied parameters.</returns>
        /// <remarks>
        /// Since reflection incurs in costly operations, when performance is important we recommend to cache the results when used repetitively.
        /// For some information on the topic see https://dejanstojanovic.net/aspnet/2019/february/making-reflection-in-net-work-faster/.
        /// </remarks>
        /// <see cref="GetPropertyInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetFieldInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetFieldsInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetMethodInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetMethodsInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        public static IEnumerable<PropertyInfo> GetPropertiesInfo(this Type type,
            string? name = default,
            string? nameStartsWith = default,
            string? nameEndsWith = default,
            string? nameContains = default,
            bool? inheritedProperties = default,
            bool? publicGetAccessor = default,
            bool? privateGetAccessor = default,
            bool? staticGetAccessor = default,
            bool? genericPropertyType = default,
            bool? genericPropertyTypeDefinition = default,
            bool? canReadProperties = default,
            bool? canWriteProperties = default,
            Type? propertyType = default,
            IEnumerable<Type>? attributes = default,
            int? genericArgumentsCount = default,
            IEnumerable<Type>? genericArguments = default)
            => from property in (inheritedProperties ?? true) ? type.GetAllRuntimeProperties() : type.GetAllDeclaredProperties()
                let propertyGenericArguments = property.PropertyType.GetGenericArguments()
                where property.Name == (name ?? property.Name)
                && (nameStartsWith == default ? true : property.Name.StartsWith(nameStartsWith!, StringComparison.Ordinal))
                && (nameEndsWith == default ? true : property.Name.EndsWith(nameEndsWith!, StringComparison.Ordinal))
                && (nameContains == default ? true : property.Name.Contains(nameContains!, StringComparison.Ordinal))
                && property.GetGetMethod(nonPublic: true)?.IsPublic == (publicGetAccessor ?? property.GetGetMethod(nonPublic: true)?.IsPublic)
                && property.GetGetMethod(nonPublic: true)?.IsPrivate == (privateGetAccessor ?? property.GetGetMethod(nonPublic: true)?.IsPrivate)
                && property.GetGetMethod(nonPublic: true)?.IsStatic == (staticGetAccessor ?? property.GetGetMethod(nonPublic: true)?.IsStatic)
                && property.PropertyType.GetTypeInfo().IsGenericType == (genericPropertyType ?? property.PropertyType.GetTypeInfo().IsGenericType)
                && property.PropertyType.GetTypeInfo().IsGenericTypeDefinition
                    == (genericPropertyTypeDefinition ?? property.PropertyType.GetTypeInfo().IsGenericTypeDefinition)
                && property.CanRead == (canReadProperties ?? property.CanRead)
                && property.CanWrite == (canWriteProperties ?? property.CanWrite)
                && property.PropertyType == (propertyType ?? property.PropertyType)
                && (attributes == default ? true : attributes.All(p => property.IsDefined(p, inheritedProperties ?? true)))
                && propertyGenericArguments.Length == (genericArgumentsCount ?? propertyGenericArguments.Length)
                && (genericArguments == default ? true : propertyGenericArguments.SequenceEqual(genericArguments))
                select property;

        /// <summary>
        /// Retrieves an object that represents a specified property. Tweak the desired parameters to filter the result.
        /// </summary>
        /// <param name="type">The type that contains the property.</param>
        /// <param name="name">
        /// The name of the property.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="nameStartsWith">
        /// The beginning of the property name.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="nameEndsWith">
        /// The end of the property name.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="nameContains">
        /// A match string in the property name.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="isInherited">
        /// A value indicating whether the property is inherited.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="hasPublicGetAccessor">
        /// A value indicating whether the property has a public get accessor.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="hasPrivateGetAccessor">
        /// A value indicating whether the property has a private get accessor.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="hasStaticGetAccessor">
        /// A value indicating whether the property has a static get accessor.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="hasGenericPropertyType">
        /// A value indicating whether the property has a generic property-type.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="hasGenericPropertyTypeDefinition">
        /// A value indicating whether the property has a generic property-type definition.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="canRead">
        /// A value indicating whether the property can be read.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="canWrite">
        /// A value indicating whether the property can be written.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="propertyType">
        /// The type of the property.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="attributes">
        /// An <see cref="IEnumerable{T}"/> that contains custom attributes defined by the property.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="genericArgumentsCount">
        /// The generic arguments count of the property.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <param name="genericArguments">
        /// An <see cref="IEnumerable{T}"/> that contains the property's generic arguments.
        /// The default value is <see langword="null"/> to not use this filter.
        /// </param>
        /// <returns>An object that represents the specified property, or null if the property is not found.</returns>
        /// <exception cref="AmbiguousMatchException">More than one property is found with the specified parameters.</exception>
        /// <remarks>
        /// Since reflection incurs in costly operations, when performance is important we recommend to cache the results when used repetitively.
        /// For some information on the topic see https://dejanstojanovic.net/aspnet/2019/february/making-reflection-in-net-work-faster/.
        /// </remarks>
        /// <see cref="GetPropertiesInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetFieldInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetFieldsInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetMethodInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        /// <seealso cref="GetMethodsInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?,
        /// Type, IEnumerable{Type}, int?, IEnumerable{Type}, int?, IEnumerable{Type})"/>
        public static PropertyInfo? GetPropertyInfo(this Type type,
            string? name = default,
            string? nameStartsWith = default,
            string? nameEndsWith = default,
            string? nameContains = default,
            bool? isInherited = default,
            bool? hasPublicGetAccessor = default,
            bool? hasPrivateGetAccessor = default,
            bool? hasStaticGetAccessor = default,
            bool? hasGenericPropertyType = default,
            bool? hasGenericPropertyTypeDefinition = default,
            bool? canRead = default,
            bool? canWrite = default,
            Type? propertyType = default,
            IEnumerable<Type>? attributes = default,
            int? genericArgumentsCount = default,
            IEnumerable<Type>? genericArguments = default)
        {
            var properties = GetPropertiesInfo(type, name, nameStartsWith, nameEndsWith, nameContains, isInherited,
                hasPublicGetAccessor, hasPrivateGetAccessor, hasStaticGetAccessor, hasGenericPropertyType, hasGenericPropertyTypeDefinition,
                canRead, canWrite, propertyType, attributes, genericArgumentsCount, genericArguments);

            if (properties.Count() > 1)
                throw new AmbiguousMatchException("More than one property is found with the specified parameters.");

            return properties.SingleOrDefault();
        }
    }
}