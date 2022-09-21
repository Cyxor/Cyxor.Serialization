using System;

namespace Cyxor.Serialization
{
    public class RawNullBackingSerializer : IBackingSerializer
    {
        public void Serialize(
            Serializer serialStream,
            object? value,
            Type? inputType,
            object? backingSerializerOptions
        ) => serialStream.SerializeRaw(default(object));

        public void Serialize<T>(Serializer serialStream, T value, object? backingSerializerOptions) =>
            serialStream.SerializeRaw(default(object));

        public T Deserialize<T>(Serializer serialStream, object? backingSerializerOptions) =>
            serialStream.DeserializeRawObject<T>();

        public object? Deserialize(Serializer serialStream, Type type, object? backingSerializerOptions) =>
            serialStream.DeserializeRawObject(type);
    }
}
