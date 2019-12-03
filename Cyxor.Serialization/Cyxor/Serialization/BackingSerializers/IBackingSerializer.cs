using System;

namespace Cyxor.Serialization
{
    public interface IBackingSerializer
    {
        void Serialize(SerializationStream serialStream, object? value, Type? type, object? backingSerializerOptions);
        void Serialize<T>(SerializationStream serialStream, T value, object? backingSerializerOptions);

        T Deserialize<T>(SerializationStream serialStream, object? backingSerializerOptions);
        object? Deserialize(SerializationStream serialStream, Type type, object? backingSerializerOptions);

#if !NET20 && !NET35 && !NET40 && !NET45 && !NETSTANDARD1_0 && !NETSTANDARD1_3 && !NETSTANDARD2_0
        public static T? CheckOptionsObject<T>(object? backingSerializerOptions) where T: class
        {
            var options = default(T);

            if (backingSerializerOptions != default)
            {
                options = backingSerializerOptions as T;

                if (options == default)
                    throw new InvalidOperationException($"Invalid parameter {nameof(backingSerializerOptions)} of type {typeof(T).Name} for {nameof(IBackingSerializer)}");
            }

            return options;
        }
#endif
    }
}