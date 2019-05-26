using System;

namespace Cyxor.Serialization
{
    public class NullBackingSerializer : IBackingSerializer
    {
        public void Serialize(SerialStream serialStream, object? value, Type? inputType, object? backingSerializerOptions)
            => serialStream.Serialize(default(object));

        public void Serialize<T>(SerialStream serialStream, T value, object? backingSerializerOptions)
            => serialStream.Serialize(default(object));

        public T Deserialize<T>(SerialStream serialStream, object? backingSerializerOptions)
            => serialStream.DeserializeObject<T>();

        public object? Deserialize(SerialStream serialStream, Type type, object? backingSerializerOptions)
            => serialStream.DeserializeObject(type);
    }
}