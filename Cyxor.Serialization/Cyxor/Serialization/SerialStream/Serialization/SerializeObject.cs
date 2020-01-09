﻿using System;
using System.IO;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        void TypeSerializeObject(object? value, Type? type, bool raw, IBackingSerializer? backingSerializer = default, object? backingSerializerOptions = default, Action<Serializer, object?>? action = null)
        {
            if (backingSerializer != default)
            {
                backingSerializer.Serialize(this, value, backingSerializerOptions);
                return;
            }

            AutoRaw = raw;
            type ??= value?.GetType() ?? typeof(object);
            (action ?? SerializerDelegateCache.GetSerializationMethod(type))(this, value);
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

            if (value == null)
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

            var circularReferencesRootObject = false;

            if (Options.HandleCircularReferences)
            {
                if (CircularReferencesIndex == 0)
                    circularReferencesRootObject = true;

                if (!raw)
                {
                    if (CircularReferencesSerializeDictionary.TryGetValue(value, out var index))
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

                CircularReferencesSerializeDictionary.Add(value, CircularReferencesIndex++);
            }

            if (Options.PrefixObjectLength)
            {
                if (SerializationStack.Count == 0)
                    PrefixObjectLengthActive = true;

                var objSerial = SerializationStack.Count > 0 ? SerializationStack.Peek() : default;
                SerializationStack.Push(new ObjectSerialization(position, raw, previous: objSerial));
            }

            try
            {
                if (!raw)
                    Serialize(ObjectProperties.EmptyMap);

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
                                var fieldValue = typeData.Fields[i].GetValueDelegate(value);

                                TypeSerializeObject(fieldValue, typeData.Fields[i].FieldInfo.FieldType, raw: false, action: typeData.Fields[i].SerializeAction);
                                
                                serializedFieldsCount++;
                            }

                        if (serializedFieldsCount == 0)
                            Serialize((byte)0);

                        prevTypeData = typeData;
                        typeData = typeData.Parent;
                        type = typeData?.Parent?.Type ?? type.BaseType!;
                    }
                }

                if (!raw && PrefixObjectLengthActive)
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
                if (PrefixObjectLengthActive)
                {
                    var objSerial = SerializationStack.Pop();

                    if (objSerial.Previous != null)
                    {
                        objSerial.Previous.Next = null;
                        objSerial.Previous = null;
                    }

                    if (SerializationStack.Count == 0)
                    {
                        SerializationStack.TrimExcess();
                        PrefixObjectLengthActive = false;
                    }
                }

                if (circularReferencesRootObject)
                {
                    CircularReferencesIndex = 0;
                    CircularReferencesSerializeDictionary.Clear();
                }
            }
        }

        public void Serialize(object? value, Type? type = default, IBackingSerializer? backingSerializer = default, object? backingSerializerOptions = default)
            => TypeSerializeObject(value, type, raw: false, backingSerializer, backingSerializerOptions);

        public void SerializeRaw(object? value, Type? type = default, IBackingSerializer? backingSerializer = default, object? backingSerializerOptions = default)
            => TypeSerializeObject(value, type, raw: true, backingSerializer, backingSerializerOptions);

        [SerializerMethodIdentifier(SerializerMethodIdentifier.SerializeObject)]
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