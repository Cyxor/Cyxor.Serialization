using System;

namespace Cyxor.Serialization
{
    public class RawNullBackingSerializer : IBackingSerializer
    {
        public void Serialize(SerializationStream serialStream, object? value, Type? inputType, object? backingSerializerOptions)
            => serialStream.SerializeRaw(default(object));

        public void Serialize<T>(SerializationStream serialStream, T value, object? backingSerializerOptions)
            => serialStream.SerializeRaw(default(object));

        public T Deserialize<T>(SerializationStream serialStream, object? backingSerializerOptions)
            => serialStream.DeserializeRawObject<T>();

        public object? Deserialize(SerializationStream serialStream, Type type, object? backingSerializerOptions)
            => serialStream.DeserializeRawObject(type);
    }
}