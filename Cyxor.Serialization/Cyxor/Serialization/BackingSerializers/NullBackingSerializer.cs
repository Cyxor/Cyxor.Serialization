using System;

namespace Cyxor.Serialization
{
    public class NullBackingSerializer : IBackingSerializer
    {
        public void Serialize(SerializationStream serialStream, object? value, Type? inputType, object? backingSerializerOptions)
            => serialStream.Serialize(default(object));

        public void Serialize<T>(SerializationStream serialStream, T value, object? backingSerializerOptions)
            => serialStream.Serialize(default(object));

        public T Deserialize<T>(SerializationStream serialStream, object? backingSerializerOptions)
            => serialStream.DeserializeObject<T>();

        public object? Deserialize(SerializationStream serialStream, Type type, object? backingSerializerOptions)
            => serialStream.DeserializeObject(type);
    }
}