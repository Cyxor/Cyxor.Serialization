using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;

#if !NET20 && !NET35 && !NET40
using System.Reflection;
#endif

namespace Cyxor.Serialization
{
#if NET20 || NET35 || NET40
    using Extensions;
#endif

    partial class SerialStream
    {
        static InvalidOperationException DataException() =>
            new InvalidOperationException(Utilities.ResourceStrings.ExceptionMessageBufferDeserializeObject);

        object? InternalTypeDeserializeObject(Type type, bool raw)
        {
            AutoRaw = raw;
            var obj = Delegate.GetFunc(type)(this);
            AutoRaw = false;

            return obj;
        }

        T InternalDeserializeObject<T>([AllowNull] T value, bool raw, bool isNullableReference, IBackingSerializer? backingSerializer = default, object? backingSerializerOptions = default)
        {
            AutoRaw = false;

            if (backingSerializer != default)
                return backingSerializer.Deserialize<T>(this, backingSerializerOptions);

            var isNullableValue = default(bool);
            var nullableValueType = default(Type);
            var type = value?.GetType() ?? typeof(T);

            if (type.FullName == default)
                throw new InvalidOperationException(Utilities.ResourceStrings.CyxorInternalException);

            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                isNullableValue = true;
                nullableValueType = type;
                type = Nullable.GetUnderlyingType(nullableValueType); //Utilities.Reflection.GetGenericArguments(type)[0];
            }

            if (IsKnownType(type))
            {
                if (raw && length == 0)
                    return ReturnDefault<T>(type, isNullableValue, isNullableReference);
                else
                {
                    var obj = InternalTypeDeserializeObject(isNullableValue ? nullableValueType! : type, raw);
                    return obj == default ? default : (T)obj;
                }
            }

            if (isNullableValue)
            {
                if (raw && length == 0)
                    return default!;

                var isNotNull = DeserializeBoolean();

                if (!isNotNull)
                    return default!;
            }

            var count = 0;

            if (!raw)
            {
                count = DeserializeByte();

                if (count == 0)
                    return ReturnDefault<T>(type, isNullableValue, isNullableReference);

                var circularReference = (ObjectProperties.CircularMap & count) == ObjectProperties.CircularMap;

                count = count == ObjectProperties.LengthMap ? DeserializeInt32()
                    : ObjectProperties.Length((byte)count);

                if (CircularReferencesActive && circularReference)
                {
                    try { return (T)CircularReferencesIndexDictionary[count]; }
                    catch { throw DataException(); }
                }

                EnsureCapacity(count, SerializerOperation.Deserialize);
            }
            else if (length == 0)
                return ReturnDefault<T>(type, isNullableValue, isNullableReference);

            if (value == default)
            {
                var instance = Activator.CreateInstance(type);

                if (instance == default)
                    throw new InvalidOperationException(Utilities.ResourceStrings.CantCreateInstanceOfType(type.Name));

                value = (T)instance;
            }

            var firstObject = false;

            if (CircularReferencesActive)
            {
                if (CircularReferencesIndex == 0)
                    firstObject = true;

                var index = CircularReferencesIndex++;
                CircularReferencesIndexDictionary.Add(index, value);
            }

            try
            {
                var currentPosition = position;

                if (value is ISerializable)
                    ((ISerializable)value).Deserialize(this);
                else
                {
                    var typeData = default(TypeData);
                    var prevTypeData = default(TypeData);

                    while (type != typeof(ValueType) && type != typeof(object))
                    {
                        if (typeData == default)
                        {
                            if (!TypesCache.TryGetValue(type, out typeData))
                            {
                                typeData = new TypeData(type);
                                _ = TypesCache.TryAdd(type, typeData);
                            }

                            if (prevTypeData != default)
                                prevTypeData.Parent = typeData;
                        }

                        var deserializedFieldsCount = 0;

                        for (var i = 0; i < typeData.Fields.Length; i++)
                            if (typeData.Fields[i].ShouldSerialize)
                            {
                                var fieldType = typeData.Fields[i].FieldInfo.FieldType;
                                var fieldValue = InternalTypeDeserializeObject(fieldType, raw: false);

                                if (typeData.Fields[i].NeedChangeCollection)
                                    fieldValue = Activator.CreateInstance(fieldType, fieldValue);

                                typeData.Fields[i].SetValue(value, fieldValue);
                                deserializedFieldsCount++;
                            }

                        if (deserializedFieldsCount == 0)
                            _ = DeserializeByte();

                        prevTypeData = typeData;
                        typeData = typeData.Parent;
                        type = typeData?.Parent?.Type ?? type.GetTypeInfo().BaseType!;
                    }
                }

                if (!raw && UseObjectSerialization && count != position - currentPosition || raw && position != length)
                    throw DataException();

                return value;
            }
            catch (Exception ex) when (!(ex is EndOfStreamException))
            {
                throw DataException();
            }
            finally
            {
                if (firstObject)
                {
                    CircularReferencesIndex = 0;
                    CircularReferencesIndexDictionary.Clear();
                }
            }

            static TDefault ReturnDefault<TDefault>(Type type, bool isNullableValue, bool isNullableReference)
            {
                if (type.GetTypeInfo().IsValueType)
                {
                    if (!isNullableValue)
                        throw new InvalidOperationException
                            (Utilities.ResourceStrings.NullValueFoundWhenDeserializingNonNullableValue(type.Name));
                }
                else
                {
                    if (!isNullableReference)
                        throw new InvalidOperationException
                            (Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(type.Name));
                }

                return default!;
            }
        }

        object? InternalDeserializeObject(Type type, bool raw, bool isNonNullableReference)
        {
            if (!isNonNullableReference)
                if (type.GetTypeInfo().IsValueType)
                    throw new InvalidOperationException
                        (Utilities.ResourceStrings.UnableToDeserializeValueTypeAsNullableReference(type.Name));

            var obj = InternalTypeDeserializeObject(type, raw);

            if (obj == default && isNonNullableReference)
                throw new InvalidOperationException
                    (Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(type.Name));

            return obj;
        }

#region Object Type

        //todo switch the isNonNullableReference for isNullableReference
        //todo add backingSerializer with Type methods

        public object DeserializeObject(Type type)
            => InternalDeserializeObject(type, raw: false, isNonNullableReference: true)!;

        public object? DeserializeNullableObject(Type type)
            => InternalDeserializeObject(type, raw: false, isNonNullableReference: false);

        public object DeserializeRawObject(Type type)
            => InternalDeserializeObject(type, raw: true, isNonNullableReference: true)!;

        public object? DeserializeNullableRawObject(Type type)
            => InternalDeserializeObject(type, raw: true, isNonNullableReference: false);

        public bool TryDeserializeObject(Type type, [NotNullWhen(true)] out object? value)
        {
            var currentPosition = position;

            try
            {
                value = InternalDeserializeObject(type, raw: false, isNonNullableReference: true);
                return true;
            }
            catch
            {
                position = currentPosition;
                value = default;
                return false;
            }
        }

        public bool TryDeserializeNullableObject(Type type, out object? value)
        {
            var currentPosition = position;

            try
            {
                value = InternalDeserializeObject(type, raw: false, isNonNullableReference: false);
                return true;
            }
            catch
            {
                position = currentPosition;
                value = default;
                return false;
            }
        }

        public object ToObject(Type type)
        {
            position = 0;
            return DeserializeRawObject(type);
        }

        public object? ToNullableObject(Type type)
        {
            position = 0;
            return DeserializeNullableRawObject(type);
        }

#endregion Object Type

#region Object T

        [return: NotNull]
        public T DeserializeObject<T>()
            => InternalDeserializeObject<T>(value: default, raw: AutoRaw, isNullableReference: false);

        public T? DeserializeNullableObject<T>() where T : class
            => InternalDeserializeObject<T>(value: default, raw: AutoRaw, isNullableReference: true);

        public T DeserializeRawObject<T>()
            => InternalDeserializeObject<T>(value: default, raw: true, isNullableReference: false);

        public T? DeserializeNullableRawObject<T>() where T : class
            => InternalDeserializeObject<T>(value: default, raw: true, isNullableReference: true);

        public T DeserializeObject<T>(T value)
            => InternalDeserializeObject(value, raw: false, isNullableReference: false);

        public T? DeserializeNullableObject<T>(T? value) where T : class
            => InternalDeserializeObject(value, raw: false, isNullableReference: true);

        public T DeserializeRawObject<T>(T value)
            => InternalDeserializeObject(value, raw: true, isNullableReference: false);

        public T? DeserializeNullableRawObject<T>(T? value) where T : class
            => InternalDeserializeObject<T>(value, raw: true, isNullableReference: true);

        public T DeserializeObject<T>(IBackingSerializer? backingSerializer, object? backingSerializerOptions = default)
            => InternalDeserializeObject<T>(value: default, raw: false, isNullableReference: false, backingSerializer, backingSerializerOptions);

        public T? DeserializeNullableObject<T>(IBackingSerializer? backingSerializer, object? backingSerializerOptions = default) where T : class
            => InternalDeserializeObject<T>(value: default, raw: false, isNullableReference: true, backingSerializer, backingSerializerOptions);

        public T DeserializeRawObject<T>(IBackingSerializer? backingSerializer, object? backingSerializerOptions = default)
            => InternalDeserializeObject<T>(value: default, raw: true, isNullableReference: false, backingSerializer, backingSerializerOptions);

        public T? DeserializeNullableRawObject<T>(IBackingSerializer? backingSerializer, object? backingSerializerOptions = default) where T : class
            => InternalDeserializeObject<T>(value: default, raw: true, isNullableReference: true, backingSerializer, backingSerializerOptions);

        public T DeserializeObject<T>(T value, IBackingSerializer? backingSerializer, object? backingSerializerOptions = default)
            => InternalDeserializeObject(value, raw: false, isNullableReference: false, backingSerializer, backingSerializerOptions);

        public T? DeserializeNullableObject<T>(T? value, IBackingSerializer? backingSerializer, object? backingSerializerOptions = default) where T : class
            => InternalDeserializeObject(value, raw: false, isNullableReference: true, backingSerializer, backingSerializerOptions);

        public T DeserializeRawObject<T>(T value, IBackingSerializer? backingSerializer, object? backingSerializerOptions = default)
            => InternalDeserializeObject(value, raw: true, isNullableReference: false, backingSerializer, backingSerializerOptions);

        public T? DeserializeNullableRawObject<T>(T? value, IBackingSerializer? backingSerializer, object? backingSerializerOptions = default) where T : class
            => InternalDeserializeObject<T>(value, raw: true, isNullableReference: true, backingSerializer, backingSerializerOptions);

#endregion Object T

#region ToConversions

        T InternalToObject<T>([AllowNull] T value, IBackingSerializer? backingSerializer, object? backingSerializerOptions = default)
        {
            var currentPosition = position;
            position = 0;

            value = InternalDeserializeObject(value, raw: true, isNullableReference: false, backingSerializer, backingSerializerOptions);

            position = currentPosition;
            return value;
        }

        // TODO: Add ToNonNullableReferenceObject overloads
        public T ToObject<T>(T value)
            => InternalToObject<T>(value, backingSerializer: default);

        public T ToObject<T>()
            => InternalToObject<T>(value: default, backingSerializer: default);

        public T ToObject<T>(IBackingSerializer? backingSerializer, object? backingSerializerOptions = default)
            => InternalToObject<T>(value: default, backingSerializer, backingSerializerOptions);

        public T ToObject<T>(T value, IBackingSerializer? backingSerializer, object? backingSerializerOptions = default)
            => InternalToObject<T>(value, backingSerializer, backingSerializerOptions);

#endregion ToConversions
    }
}