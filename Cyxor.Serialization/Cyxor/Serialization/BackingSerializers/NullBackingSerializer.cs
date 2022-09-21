using System;

namespace Cyxor.Serialization
{
    public class NullBackingSerializer : IBackingSerializer
    {
        public void Serialize(
            Serializer serialStream,
            object? value,
            Type? inputType,
            object? backingSerializerOptions
        ) => serialStream.Serialize(default(object));

        public void Serialize<T>(Serializer serialStream, T value, object? backingSerializerOptions) =>
            serialStream.Serialize(default(object));

        public T Deserialize<T>(Serializer serialStream, object? backingSerializerOptions) =>
            serialStream.DeserializeObject<T>();

        public object? Deserialize(Serializer serialStream, Type type, object? backingSerializerOptions) =>
            serialStream.DeserializeObject(type);
    }
}
