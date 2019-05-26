using System;

namespace Cyxor.Serialization
{
    public class RawNullBackingSerializer : IBackingSerializer
    {
        public void Serialize(SerialStream serialStream, object? value, Type? inputType, object? backingSerializerOptions)
            => serialStream.SerializeRaw(default(object));

        public void Serialize<T>(SerialStream serialStream, T value, object? backingSerializerOptions)
            => serialStream.SerializeRaw(default(object));

        public T Deserialize<T>(SerialStream serialStream, object? backingSerializerOptions)
            => serialStream.DeserializeRawObject<T>();

        public object? Deserialize(SerialStream serialStream, Type type, object? backingSerializerOptions)
            => serialStream.DeserializeRawObject(type);
    }
}