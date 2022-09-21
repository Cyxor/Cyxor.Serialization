using System;
using System.Text.Json;

namespace Cyxor.Serialization
{
    public class JsonBackingSerializer : IBackingSerializer
    {
        public void Serialize(Serializer serialStream, object? value, Type? inputType, object? backingSerializerOptions)
        {
            var jsonSerializerOptions = IBackingSerializer.CheckOptionsObject<JsonSerializerOptions>(
                backingSerializerOptions
            );
            using var utf8JsonWriter = new Utf8JsonWriter(serialStream);
            JsonSerializer.Serialize(
                utf8JsonWriter,
                value,
                inputType ?? value?.GetType() ?? typeof(object),
                jsonSerializerOptions
            );
        }

        public void Serialize<T>(Serializer serialStream, T value, object? backingSerializerOptions)
        {
            var jsonSerializerOptions = backingSerializerOptions == null
                ? default
                : IBackingSerializer.ValidateOptions<JsonSerializerOptions>(backingSerializerOptions);

            using var utf8JsonWriter = new Utf8JsonWriter(serialStream);

            JsonSerializer.Serialize(utf8JsonWriter, value, jsonSerializerOptions);
        }

        public T Deserialize<T>(Serializer serialStream, object? backingSerializerOptions)
        {
            var jsonSerializerOptions = backingSerializerOptions == null
                ? default
                : IBackingSerializer.ValidateOptions<JsonSerializerOptions>(backingSerializerOptions);

            var utf8JsonReader = new Utf8JsonReader(
                serialStream.AsReadOnlySpan<byte>(
                    serialStream.Int32Position,
                    serialStream.Int32Length - serialStream.Int32Position
                )
            );

            var value = JsonSerializer.Deserialize<T>(ref utf8JsonReader, jsonSerializerOptions);

            serialStream.Position += utf8JsonReader.BytesConsumed;

            return value;
        }

        public object? Deserialize(Serializer serialStream, Type type, object? backingSerializerOptions)
        {
            var jsonSerializerOptions = backingSerializerOptions == null
                ? default
                : IBackingSerializer.ValidateOptions<JsonSerializerOptions>(backingSerializerOptions);

            var utf8JsonReader = new Utf8JsonReader(
                serialStream.AsReadOnlySpan<byte>(
                    serialStream.Int32Position,
                    serialStream.Int32Length - serialStream.Int32Position
                )
            );

            var value = JsonSerializer.Deserialize(ref utf8JsonReader, type, jsonSerializerOptions);

            serialStream.Position += utf8JsonReader.BytesConsumed;

            return value;
        }
    }
}
