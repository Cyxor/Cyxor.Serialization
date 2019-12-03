using System.Diagnostics.CodeAnalysis;

namespace Cyxor.Serialization
{
    partial class SerializationStream
    {
        public SerializationStream DeserializeSerialStream()
            => new SerializationStream(DeserializeBytes());

        public SerializationStream? DeserializeNullableSerialStream()
        {
            var bytes = DeserializeNullableBytes();
            return bytes == default ? default : new SerializationStream(bytes);
        }

        public SerializationStream DeserializeRawSerialStream()
            => new SerializationStream(DeserializeRawBytes());

        public SerializationStream? DeserializeNullableRawSerialStream()
        {
            var bytes = DeserializeNullableRawBytes();
            return bytes == default ? default : new SerializationStream(bytes);
        }

        public bool TryDeserializeSerialStream([NotNullWhen(true)] out SerializationStream? value)
        {
            value = default;

            if (!TryDeserializeBytes(out var bytes))
                return false;

            try
            {
                value = new SerializationStream(bytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryDeserializeNullableSerialStream(out SerializationStream? value)
        {
            value = default;

            if (!TryDeserializeNullableBytes(out var bytes))
                return false;

            if (bytes == default)
                return true;

            try
            {
                value = new SerializationStream(bytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public SerializationStream ToSerialStream()
        {
            position = 0;
            return DeserializeRawSerialStream();
        }

        public SerializationStream? ToNullableSerialStream()
        {
            position = 0;
            return DeserializeNullableRawSerialStream();
        }
    }
}