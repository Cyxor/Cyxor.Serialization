using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cyxor.Extensions;

public static partial class ReflectionExtensions
{
    /// <summary>
    /// Retrieves a collection that represents all fields defined on a specified type when using the default parameters, 
    /// including inherited, non-public, instance, and static fields. Tweak the desired parameters to filter the result.
    /// </summary>
    /// <param name="type">The type that contains the fields.</param>
    /// <param name="name">
    /// The name of the fields.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="nameStartsWith">
    /// The beginning of the fields name.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="nameEndsWith">
    /// The end of the fields name.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="nameContains">
    /// A match string in the fields name.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="inheritedFields">
    /// A value indicating whether to filter by inherited fields.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="publicFields">
    /// A value indicating whether to filter by public fields.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="privateFields">
    /// A value indicating whether to filter by private fields.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="staticFields">
    /// A value indicating whether to filter by static fields.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="genericFieldType">
    /// A value indicating whether to filter by generic FieldType.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="genericFieldTypeDefinition">
    /// A value indicating whether to filter by generic FieldType definition.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="literalFields">
    /// A value indicating whether to filter by literal fields.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="initOnlyFields">
    /// A value indicating whether to filter by init only fields.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="fieldType">
    /// The type of the fields.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="attributes">
    /// An <see cref="IEnumerable{T}"/> that contains custom attributes defined by the fields.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="genericArgumentsCount">
    /// The generic arguments count of the fields.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="genericArguments">
    /// An <see cref="IEnumerable{T}"/> that contains the fields's generic arguments.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <returns>A collection of fields for the specified type filtered by the supplied parameters.</returns>
    /// <remarks>
    /// Since reflection incurs in costly operations, when performance is important we recommend to cache the results when used repetitively.
    /// For some information on the topic see https://dejanstojanovic.net/aspnet/2019/february/making-reflection-in-net-work-faster/.
    /// </remarks>
    /// <see cref="GetFieldInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
    /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
    /// <seealso cref="GetPropertyInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
    /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
    /// <seealso cref="GetPropertiesInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
    /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
    /// <seealso cref="GetMethodInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?,
    /// Type, IEnumerable{Type}, int?, IEnumerable{Type}, int?, IEnumerable{Type})"/>
    /// <seealso cref="GetMethodsInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?,
    /// Type, IEnumerable{Type}, int?, IEnumerable{Type}, int?, IEnumerable{Type})"/>
    public static IEnumerable<FieldInfo> GetFieldsInfo(
        this Type type,
        string? name = default,
        string? nameStartsWith = default,
        string? nameEndsWith = default,
        string? nameContains = default,
        bool? inheritedFields = default,
        bool? publicFields = default,
        bool? privateFields = default,
        bool? staticFields = default,
        bool? genericFieldType = default,
        bool? genericFieldTypeDefinition = default,
        bool? literalFields = default,
        bool? initOnlyFields = default,
        Type? fieldType = default,
        IEnumerable<Type>? attributes = default,
        int? genericArgumentsCount = default,
        IEnumerable<Type>? genericArguments = default
    ) =>

            from field in (inheritedFields ?? true) ? type.GetRuntimeFields() : type.GetTypeInfo().DeclaredFields
            let fieldGenericArguments = field.FieldType.GetGenericArguments()
            where
                field.Name == (name ?? field.Name)
                && (nameStartsWith == default
                    ? true
                    : field.Name.StartsWith(nameStartsWith!, StringComparison.Ordinal))
                && (nameEndsWith == default ? true : field.Name.EndsWith(nameEndsWith!, StringComparison.Ordinal))
                && (nameContains == default ? true : field.Name.Contains(nameContains!, StringComparison.Ordinal))
                && field.IsPublic == (publicFields ?? field.IsPublic)
                && field.IsPrivate == (privateFields ?? field.IsPrivate)
                && field.IsStatic == (staticFields ?? field.IsStatic)
                && field.FieldType.GetTypeInfo().IsGenericType
                == (genericFieldType ?? field.FieldType.GetTypeInfo().IsGenericType)
                && field.FieldType.GetTypeInfo().IsGenericTypeDefinition
                == (genericFieldTypeDefinition ?? field.FieldType.GetTypeInfo().IsGenericTypeDefinition)
                && field.IsLiteral == (literalFields ?? field.IsLiteral)
                && field.IsInitOnly == (initOnlyFields ?? field.IsInitOnly)
                && field.FieldType == (fieldType ?? field.FieldType)
                && (attributes == default ? true : attributes.All(p => field.IsDefined(p, inheritedFields ?? true)))
                && fieldGenericArguments.Length == (genericArgumentsCount ?? fieldGenericArguments.Length)
                && (genericArguments == default ? true : fieldGenericArguments.SequenceEqual(genericArguments))
            select field;

    /// <summary>
    /// Retrieves an object that represents a specified field. Tweak the desired parameters to filter the result.
    /// </summary>
    /// <param name="type">The type that contains the field.</param>
    /// <param name="name">
    /// The name of the field.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="nameStartsWith">
    /// The beginning of the field name.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="nameEndsWith">
    /// The end of the field name.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="nameContains">
    /// A match string in the field name.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="isInherited">
    /// A value indicating whether the field is inherited.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="isPublic">
    /// A value indicating whether the field is public.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="isPrivate">
    /// A value indicating whether the field is private.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="isStatic">
    /// A value indicating whether the field is static.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="hasGenericFieldType">
    /// A value indicating whether the field has a generic field-type.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="hasGenericFieldTypeDefinition">
    /// A value indicating whether the field has a generic field-type definition.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="isLiteral">
    /// A value indicating whether the field is literal.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="isInitOnly">
    /// A value indicating whether the field is init only.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="fieldType">
    /// The type of the field.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="attributes">
    /// An <see cref="IEnumerable{T}"/> that contains custom attributes defined by the field.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="genericArgumentsCount">
    /// The generic arguments count of the field.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <param name="genericArguments">
    /// An <see cref="IEnumerable{T}"/> that contains the field's generic arguments.
    /// The default value is <see langword="null"/> to not use this filter.
    /// </param>
    /// <returns>An object that represents the specified field, or null if the field is not found.</returns>
    /// <exception cref="AmbiguousMatchException">More than one field is found with the specified parameters.</exception>
    /// <remarks>
    /// Since reflection incurs in costly operations, when performance is important we recommend to cache the results when used repetitively.
    /// For some information on the topic see https://dejanstojanovic.net/aspnet/2019/february/making-reflection-in-net-work-faster/.
    /// </remarks>
    /// <see cref="GetFieldsInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
    /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
    /// <seealso cref="GetPropertyInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
    /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
    /// <seealso cref="GetPropertiesInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?,
    /// Type, IEnumerable{Type}, int?, IEnumerable{Type})"/>
    /// <seealso cref="GetMethodInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?,
    /// Type, IEnumerable{Type}, int?, IEnumerable{Type}, int?, IEnumerable{Type})"/>
    /// <seealso cref="GetMethodsInfo(Type, string, string, string, string, bool?, bool?, bool?, bool?, bool?, bool?,
    /// Type, IEnumerable{Type}, int?, IEnumerable{Type}, int?, IEnumerable{Type})"/>
    public static FieldInfo? GetFieldInfo(
        this Type type,
        string? name = default,
        string? nameStartsWith = default,
        string? nameEndsWith = default,
        string? nameContains = default,
        bool? isInherited = default,
        bool? isPublic = default,
        bool? isPrivate = default,
        bool? isStatic = default,
        bool? hasGenericFieldType = default,
        bool? hasGenericFieldTypeDefinition = default,
        bool? isLiteral = default,
        bool? isInitOnly = default,
        Type? fieldType = default,
        IEnumerable<Type>? attributes = default,
        int? genericArgumentsCount = default,
        IEnumerable<Type>? genericArguments = default
    )
    {
        var fields = GetFieldsInfo(
            type,
            name,
            nameStartsWith,
            nameEndsWith,
            nameContains,
            isInherited,
            isPublic,
            isPrivate,
            isStatic,
            hasGenericFieldType,
            hasGenericFieldTypeDefinition,
            isLiteral,
            isInitOnly,
            fieldType,
            attributes,
            genericArgumentsCount,
            genericArguments
        );

        if (fields.Count() > 1)
            throw new AmbiguousMatchException("More than one field is found with the specified parameters.");

        return fields.SingleOrDefault();
    }
}
