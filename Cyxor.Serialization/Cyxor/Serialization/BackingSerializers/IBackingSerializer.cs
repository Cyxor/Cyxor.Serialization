using System;
using System.Diagnostics.CodeAnalysis;

namespace Cyxor.Serialization
{
    public interface IBackingSerializer
    {
        void Serialize(Serializer serialStream, object? value, Type? type, object? backingSerializerOptions);
        void Serialize<T>(Serializer serialStream, T value, object? backingSerializerOptions);

        T Deserialize<T>(Serializer serialStream, object? backingSerializerOptions);
        object? Deserialize(Serializer serialStream, Type type, object? backingSerializerOptions);

        public static T ValidateOptions<T>(object backingSerializerOptions)
        {
            if (backingSerializerOptions is T options)
                return options;

            throw new InvalidOperationException(
                $"Invalid serializer options for {nameof(IBackingSerializer)}, expected type is {typeof(T).Name}"
            );
        }
    }
}
