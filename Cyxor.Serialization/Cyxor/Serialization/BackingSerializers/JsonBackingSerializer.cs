#if !NET20 && !NET35 && !NET40 && !NET45 && !NETSTANDARD1_0 && !NETSTANDARD1_3 && !NETSTANDARD2_0 && !NETSTANDARD2_1

using System;
using System.Text.Json;

namespace Cyxor.Serialization
{
    public class JsonBackingSerializer : IBackingSerializer
    {
        public void Serialize(SerialStream serialStream, object? value, Type? inputType, object? backingSerializerOptions)
        {
            var jsonSerializerOptions = IBackingSerializer.CheckOptionsObject<JsonSerializerOptions>(backingSerializerOptions);
            using var utf8JsonWriter = new Utf8JsonWriter(serialStream);
            JsonSerializer.Serialize(utf8JsonWriter, value, inputType ?? value?.GetType() ?? typeof(object), jsonSerializerOptions);
        }

        public void Serialize<T>(SerialStream serialStream, T value, object? backingSerializerOptions)
        {
            var jsonSerializerOptions = IBackingSerializer.CheckOptionsObject<JsonSerializerOptions>(backingSerializerOptions);
            using var utf8JsonWriter = new Utf8JsonWriter(serialStream);
            JsonSerializer.Serialize(utf8JsonWriter, value, jsonSerializerOptions);
        }

        public T Deserialize<T>(SerialStream serialStream, object? backingSerializerOptions)
        {
            var jsonSerializerOptions = IBackingSerializer.CheckOptionsObject<JsonSerializerOptions>(backingSerializerOptions);
            var readOnlySpan = new ReadOnlySpan<byte>(serialStream.GetBuffer(), serialStream.Int32Position, serialStream.Int32Length);
            var utf8JsonReader = new Utf8JsonReader(readOnlySpan);
            var value = JsonSerializer.Deserialize<T>(ref utf8JsonReader, jsonSerializerOptions);
            serialStream.Position += utf8JsonReader.BytesConsumed;
            return value;
        }

        public object? Deserialize(SerialStream serialStream, Type type, object? backingSerializerOptions)
        {
            var jsonSerializerOptions = IBackingSerializer.CheckOptionsObject<JsonSerializerOptions>(backingSerializerOptions);
            var readOnlySpan = new ReadOnlySpan<byte>(serialStream.GetBuffer(), serialStream.Int32Position, serialStream.Int32Length);
            var utf8JsonReader = new Utf8JsonReader(readOnlySpan);
            var value = JsonSerializer.Deserialize(ref utf8JsonReader, type, jsonSerializerOptions);
            serialStream.Position += utf8JsonReader.BytesConsumed;
            return value;
        }
    }
}
#endif