using System;
using System.IO;

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
        void TypeSerializeObject(object? value, Type? type, bool raw, IBackingSerializer? backingSerializer = default, object? backingSerializerOptions = default)
        {
            if (backingSerializer != default)
            {
                backingSerializer.Serialize(this, value, backingSerializerOptions);
                return;
            }

            AutoRaw = raw;
            Delegate.GetAction(type ?? value?.GetType() ?? typeof(object))(this, value);
            AutoRaw = false;
        }

        void InternalSerializeObject<T>(T value, bool raw, IBackingSerializer? backingSerializer = default, object? backingSerializerOptions = default)
        {
            AutoRaw = false;

            if (backingSerializer != default)
            {
                backingSerializer.Serialize(this, value, backingSerializerOptions);
                return;
            }

            if (value == default)
            {
                if (!raw)
                    Serialize((byte)0);

                return;
            }

            var type = value.GetType();
            var serializable = value as ISerializable;

            if (serializable == default && IsKnownType(type))
            {
                TypeSerializeObject(value, type, raw);
                return;
            }

            if (CircularReferencesActive)
            {
                if (!raw)
                {
                    if (CircularReferencesDictionary.TryGetValue(value, out var index))
                    {
                        if (index + 1 < ObjectProperties.MaxLength)
                            Serialize((byte)(ObjectProperties.CircularMap | ((index + 1) << 2)));
                        else
                        {
                            Serialize((byte)(ObjectProperties.CircularMap | ObjectProperties.LengthMap));
                            Serialize(index + 1);
                        }

                        return;
                    }
                }

                CircularReferencesDictionary.Add(value, CircularReferencesIndex++);
            }

            if (UseObjectSerialization)
            {
                if (SerializationStack.Count == 0)
                    ObjectSerializationActive = true;

                var objSerial = SerializationStack.Count > 0 ? SerializationStack.Peek() : default;
                SerializationStack.Push(new ObjectSerialization(position, raw, previous: objSerial));
            }

            try
            {
                if (!raw && UseObjectSerialization)
                    Serialize((byte)0);
                else if (!raw)
                    Serialize((byte)1);

                if (serializable != default)
                    serializable.Serialize(this);
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

                        var serializedFieldsCount = 0;

                        for (var i = 0; i < typeData.Fields.Length; i++)
                            if (typeData.Fields[i].ShouldSerialize)
                            {
                                var fieldValue = typeData.Fields[i].GetValue(value);

                                TypeSerializeObject(fieldValue, typeData.Fields[i].FieldInfo.FieldType, raw: false);
                                serializedFieldsCount++;
                            }

                        if (serializedFieldsCount == 0)
                            Serialize((byte)0);

                        prevTypeData = typeData;
                        typeData = typeData.Parent;
                        type = typeData?.Parent?.Type ?? type.GetTypeInfo().BaseType!;
                    }
                }

                if (!raw && UseObjectSerialization)
                {
                    var finalPosition = position;
                    var serializationObject = SerializationStack.Peek();
                    position = serializationObject.BufferPosition;
                    SerializeOp(finalPosition - (position + serializationObject.PositionLenght));
                    position = finalPosition;
                }
            }
            catch (Exception ex) when (!(ex is EndOfStreamException))
            {
                throw DataException();
            }
            finally
            {
                if (UseObjectSerialization)
                {
                    var objSerial = SerializationStack.Pop();

                    if (objSerial.Previous != default)
                    {
                        objSerial.Previous.Next = default;
                        objSerial.Previous = default;
                    }

                    if (SerializationStack.Count == 0)
                    {
                        if (CircularReferencesActive)
                        {
                            CircularReferencesIndex = 0;
                            CircularReferencesDictionary.Clear();
                        }

                        SerializationStack.TrimExcess();

                        ObjectSerializationActive = false;
                    }
                }
            }
        }

        public void Serialize(object? value, Type? type = default, IBackingSerializer? backingSerializer = default, object? backingSerializerOptions = default)
            => TypeSerializeObject(value, type, raw: false, backingSerializer, backingSerializerOptions);

        public void SerializeRaw(object? value, Type? type = default, IBackingSerializer? backingSerializer = default, object? backingSerializerOptions = default)
            => TypeSerializeObject(value, type, raw: true, backingSerializer, backingSerializerOptions);

        public void Serialize<T>(T value)
            => InternalSerializeObject(value, raw: false);

        public void SerializeRaw<T>(T value)
            => InternalSerializeObject(value, raw: true);

        public void Serialize<T>(T value, IBackingSerializer backingSerializer, object? backingSerializerOptions = default)
            => InternalSerializeObject(value, raw: false, backingSerializer, backingSerializerOptions);

        public void SerializeRaw<T>(T value, IBackingSerializer backingSerializer, object? backingSerializerOptions = default)
            => InternalSerializeObject(value, raw: true, backingSerializer, backingSerializerOptions);
    }
}