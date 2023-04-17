using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Cyxor.Serialization;

using Extensions;

using MethodDictionary = Dictionary<Type, MethodInfo>;

partial class Serializer
{
    delegate void SerializeUnmanaged<T>(T value);

    static class MyClass<T>
    {
        public static readonly SerializeUnmanaged<T> SerializeUnmanagedDelegate =
            (SerializeUnmanaged<T>)SerializeUnmanagedInfo.Method.CreateDelegate(typeof(SerializeUnmanaged<T>));
    }

    static readonly bool IsLittleEndian = BitConverter.IsLittleEndian;

    static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Create();

    static readonly IEnumerable<MethodInfo> SerializerMethods = typeof(Serializer).GetMethodsInfo();

    static readonly MethodDictionary SerializeMethods = (
        from method in typeof(Serializer).GetMethodsInfo(
            name: nameof(SerializerOperation.Serialize),
            publicMethods: true,
            parametersCount: 1
        )
        let parameterType = method.GetParameters().Single().ParameterType
        where !parameterType.IsPointer
        orderby parameterType.Name
        select new KeyValuePair<Type, MethodInfo>(parameterType, method)).ToDictionary(p => p.Key, p => p.Value);

    static readonly MethodDictionary DeserializeMethods = GetDeserializeMethods();

    public static readonly IEnumerable<Type> SupportedTypes = SerializeMethods.Keys;

    static readonly (Type Type, MethodInfo Method) SerializeEnumInfo = SerializerMethods.Where(
            p =>
                p.GetCustomAttribute<SerializerMethodIdentifierAttribute>()?.Identifier
                == SerializerMethodIdentifier.SerializeEnum
        )
        .Select(p => (typeof(Enum), p))
        .Single();

    static readonly (Type Type, MethodInfo Method) DeserializeEnumInfo = SerializerMethods.Where(
            p =>
                p.GetCustomAttribute<SerializerMethodIdentifierAttribute>()?.Identifier
                == SerializerMethodIdentifier.DeserializeEnum
        )
        .Select(p => (typeof(Enum), p))
        .Single();

    static readonly (Type Type, MethodInfo Method) SerializeUnmanagedInfo = SerializerMethods.Where(
            p =>
                p.GetCustomAttribute<SerializerMethodIdentifierAttribute>()?.Identifier
                == SerializerMethodIdentifier.SerializeUnmanaged
        )
        .Select(p => (typeof(Utilities.Unmanaged), p))
        .Single();

    static readonly (Type Type, MethodInfo Method) DeserializeUnmanagedInfo = SerializerMethods.Where(
            p =>
                p.GetCustomAttribute<SerializerMethodIdentifierAttribute>()?.Identifier
                == SerializerMethodIdentifier.DeserializeUnmanaged
        )
        .Select(p => (typeof(Utilities.Unmanaged), p))
        .Single();

    static readonly (Type Type, MethodInfo Method) SerializeObjectInfo = SerializeMethods.Where(
            p =>
                p.Value.GetCustomAttribute<SerializerMethodIdentifierAttribute>()?.Identifier
                == SerializerMethodIdentifier.SerializeObject
        )
        .Select(p => (p.Key, p.Value))
        .Single();

    static readonly (Type Type, MethodInfo Method) DeserializeObjectInfo = DeserializeMethods.Where(
            p =>
                p.Value.GetCustomAttribute<SerializerMethodIdentifierAttribute>()?.Identifier
                == SerializerMethodIdentifier.DeserializeObject
        )
        .Select(p => (p.Key, p.Value))
        .Single();

    static readonly (Type Type, MethodInfo Method) SerializeIEnumerableInfo = SerializeMethods.Where(
            p =>
                p.Value.GetCustomAttribute<SerializerMethodIdentifierAttribute>()?.Identifier
                == SerializerMethodIdentifier.SerializeIEnumerable
        )
        .Select(p => (p.Key, p.Value))
        .Single();

    static readonly (Type Type, MethodInfo Method) DeserializeIEnumerableInfo = DeserializeMethods.Where(
            p =>
                p.Value.GetCustomAttribute<SerializerMethodIdentifierAttribute>()?.Identifier
                == SerializerMethodIdentifier.DeserializeIEnumerable
        )
        .Select(p => (p.Key, p.Value))
        .Single();

    static readonly (Type Type, MethodInfo Method) SerializeIDictionaryInfo = SerializeMethods.Where(
            p =>
                p.Value.GetCustomAttribute<SerializerMethodIdentifierAttribute>()?.Identifier
                == SerializerMethodIdentifier.SerializeIDictionary
        )
        .Select(p => (p.Key, p.Value))
        .Single();

    static readonly (Type Type, MethodInfo Method) DeserializeIDictionaryInfo = DeserializeMethods.Where(
            p =>
                p.Value.GetCustomAttribute<SerializerMethodIdentifierAttribute>()?.Identifier
                == SerializerMethodIdentifier.DeserializeIDictionary
        )
        .Select(p => (p.Key, p.Value))
        .Single();

    static readonly (Type Type, MethodInfo Method) SerializeIGroupingInfo = SerializeMethods.Where(
            p =>
                p.Value.GetCustomAttribute<SerializerMethodIdentifierAttribute>()?.Identifier
                == SerializerMethodIdentifier.SerializeIGrouping
        )
        .Select(p => (p.Key, p.Value))
        .Single();

    static readonly (Type Type, MethodInfo Method) DeserializeIGroupingInfo = DeserializeMethods.Where(
            p =>
                p.Value.GetCustomAttribute<SerializerMethodIdentifierAttribute>()?.Identifier
                == SerializerMethodIdentifier.DeserializeIGrouping
        )
        .Select(p => (p.Key, p.Value))
        .Single();

    static readonly MethodInfo IsReferenceOrContainsReferencesMethodInfo =
        typeof(RuntimeHelpers).GetMethodInfo(
            nameof(RuntimeHelpers.IsReferenceOrContainsReferences),
            isPublic: true,
            parametersCount: 0,
            isGenericMethodDefinition: true,
            genericArgumentsCount: 1
        )!;

    static Serializer()
    {
        //Expression.Lambda(null).Compile().


        foreach (var deserializeMethod in DeserializeMethods) if (!deserializeMethod.Value.IsGenericMethodDefinition)
                _ = SerializerDelegateCache.GetDeserializationMethod(deserializeMethod.Key);

        foreach (var serializeMethod in SerializeMethods) if (!serializeMethod.Value.IsGenericMethodDefinition)
                _ = SerializerDelegateCache.GetSerializationMethod(serializeMethod.Key);
    }

    internal static ConcurrentDictionary<Type, TypeData> TypesCache = new ConcurrentDictionary<Type, TypeData>();
    internal static ConcurrentDictionary<Type, bool> KnownTypesCache = new ConcurrentDictionary<Type, bool>();

    public static bool IsKnownType(Type type)
    {
        if (KnownTypesCache.TryGetValue(type, out var result))
            return result;

        result = type.IsPrimitive
        || type.IsEnum
        || type.IsArray
        || type == typeof(string)
        || type == typeof(decimal)
        || type == typeof(Guid)
        || type == typeof(TimeSpan)
        || type == typeof(DateTime)
        || type == typeof(DateTimeOffset)
        || type == typeof(BitSerializer)
        || type == typeof(Uri)
        || type == typeof(Serializer)
        || type == typeof(MemoryStream)
        || type.IsInterfaceImplemented<IEnumerable>();

        _ = KnownTypesCache.TryAdd(type, result);

        return result;
    }

    static MethodDictionary GetDeserializeMethods()
    {
        var nonNullableOperationName = nameof(SerializerOperation.Deserialize);
        var nullableOperationName = $"{nonNullableOperationName}{nameof(Nullable)}";

        var deserializeMethods = (
            from method in typeof(Serializer).GetMethodsInfo(
                nameStartsWith: nameof(SerializerOperation.Deserialize),
                publicMethods: true,
                parametersCount: 0
            )
            let returnType = method.ReturnType
            let operationName = returnType.GetTypeInfo().IsValueType
                ? nonNullableOperationName
                : nullableOperationName
            where
                method.Name.StartsWith(operationName, StringComparison.OrdinalIgnoreCase)
                && !method.Name.Contains("raw", StringComparison.OrdinalIgnoreCase)
                && !method.Name.EndsWith("enum", StringComparison.OrdinalIgnoreCase)
                && !method.Name.EndsWith("unmanaged", StringComparison.OrdinalIgnoreCase)
                && !method.Name.Contains("collection", StringComparison.OrdinalIgnoreCase)
                && !method.Name.Contains("dictionary", StringComparison.OrdinalIgnoreCase)
                && !method.Name.Contains("compressed", StringComparison.OrdinalIgnoreCase)
            orderby returnType.Name
            select new KeyValuePair<Type, MethodInfo>(returnType, method)).ToDictionary(p => p.Key, p => p.Value);

        // enum
        SerializeMethods.Add(SerializeEnumInfo.Type, SerializeEnumInfo.Method);
        deserializeMethods.Add(DeserializeEnumInfo.Type, DeserializeEnumInfo.Method);

        // unmanaged
        SerializeMethods.Add(SerializeUnmanagedInfo.Type, SerializeUnmanagedInfo.Method);
        deserializeMethods.Add(SerializeUnmanagedInfo.Type, DeserializeUnmanagedInfo.Method);

        var notMappedTypes = SerializeMethods.Keys.Except(deserializeMethods.Keys, NameEqualityComparer.Instance);

        var sb = new StringBuilder();
        _ = sb.Append(Utilities.ResourceStrings.CyxorInternalException);

        var notMappedTypesCount = notMappedTypes.Count();

        if (notMappedTypesCount > 0)
        {
            var isAre = notMappedTypesCount == 1 ? "is" : "are";
            var typeTypes = notMappedTypesCount == 1 ? "type" : "types";

            _ = sb.Append($", there {isAre} '{notMappedTypesCount}' unmapped {typeTypes}:");

            var count = 0;

            foreach (var notMappedType in notMappedTypes) _ = sb.Append($" {++count}- {notMappedType.Name}");
        }

        if (deserializeMethods.Count != SerializeMethods.Count)
            throw new InvalidOperationException(sb.ToString());

        //if (nullableGenericType == typeof(SerialStream))
        //    throw new InvalidOperationException(Utilities.ResourceStrings.CyxorInternalException);

        return deserializeMethods;
    }

    public static string GenerateSerializationSchema()
    {
        var result = new StringBuilder();

        foreach (var method in SerializeMethods)
        {
            _ = result.AppendLine(method.Value.ToString());

            _ = DeserializeMethods.ContainsKey(method.Key)
                ? result.AppendLine($"'-->{DeserializeMethods[method.Key]}{Environment.NewLine}")
                : result.AppendLine("Failed");
        }

        return result.ToString();
    }

    internal static MethodInfo GetSerializerMethod(Type type, SerializerOperation operation)
    {
        var dictionary = operation == SerializerOperation.Serialize ? SerializeMethods : DeserializeMethods;

        if (type.IsByRef)
            type = type.GetElementType()!;

        var suitableType = type;
        var genericArgumentsType = default(Type[]);

        if (type.IsInterfaceImplemented<ISerializable>())
        {
            suitableType = typeof(ISerializable);
            genericArgumentsType = new Type[] { type };
        }
        else if (type.IsEnum)
        {
            suitableType = typeof(Enum);
            genericArgumentsType = new Type[] { type };
        }
        else if (type.IsArray && type.GetElementType() != typeof(byte) && type.GetElementType() != typeof(char))
        {
            suitableType = typeof(Array);
            genericArgumentsType = new Type[] { type.GetElementType()! };
        }
        else if (type.IsInterfaceImplemented(typeof(IEnumerable<>)) && type.IsGenericType)
        {
            var genericArguments = type.GetGenericArguments();

            if (genericArguments.Length == 2)
            {
                if (type.IsInterfaceImplemented(typeof(IGrouping<,>)))
                    suitableType = operation == SerializerOperation.Serialize
                        ? SerializeIGroupingInfo.Type
                        : DeserializeIGroupingInfo.Type;
                else if (type.IsInterfaceImplemented(typeof(IDictionary<,>)))
                    suitableType = operation == SerializerOperation.Serialize
                        ? SerializeIDictionaryInfo.Type
                        : DeserializeIDictionaryInfo.Type;
            }
            else
                suitableType = genericArguments.Length == 1 && genericArguments.Single() == typeof(KeyValuePair<,>)
                    ? operation == SerializerOperation.Serialize
                            ? SerializeIDictionaryInfo.Type
                            : DeserializeIDictionaryInfo.Type
                    : operation == SerializerOperation.Serialize
                            ? SerializeIEnumerableInfo.Type
                            : DeserializeIEnumerableInfo.Type;
        }

        if (!dictionary.TryGetValue(suitableType, out var method))
        {
            var isReferenceOrContainsReferencesMethodInfo = IsReferenceOrContainsReferencesMethodInfo.MakeGenericMethod(
                type
            );

            method = !(bool)isReferenceOrContainsReferencesMethodInfo.Invoke(null, null)!
                ? operation == SerializerOperation.Serialize
                        ? SerializeUnmanagedInfo.Method
                        : DeserializeUnmanagedInfo.Method
                : operation == SerializerOperation.Serialize
                        ? SerializeObjectInfo.Method
                        : DeserializeObjectInfo.Method;

            genericArgumentsType = new Type[] { type };
        }

        if (method.IsGenericMethodDefinition)
            method = method.MakeGenericMethod(genericArgumentsType ?? type.GetGenericArguments());

        return method;
    }
}
