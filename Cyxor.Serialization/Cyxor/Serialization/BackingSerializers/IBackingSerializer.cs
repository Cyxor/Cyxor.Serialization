using System;

namespace Cyxor.Serialization
{
    public interface IBackingSerializer
    {
        void Serialize(SerialStream serialStream, object? value, Type? type, object? backingSerializerOptions);
        void Serialize<T>(SerialStream serialStream, T value, object? backingSerializerOptions);

        T Deserialize<T>(SerialStream serialStream, object? backingSerializerOptions);
        object? Deserialize(SerialStream serialStream, Type type, object? backingSerializerOptions);

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