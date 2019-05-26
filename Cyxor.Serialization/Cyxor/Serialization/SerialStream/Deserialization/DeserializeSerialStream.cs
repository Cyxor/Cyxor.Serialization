using System.Diagnostics.CodeAnalysis;

namespace Cyxor.Serialization
{
    partial class SerialStream
    {
        public SerialStream DeserializeSerialStream()
            => new SerialStream(DeserializeBytes());

        public SerialStream? DeserializeNullableSerialStream()
        {
            var bytes = DeserializeNullableBytes();
            return bytes == default ? default : new SerialStream(bytes);
        }

        public SerialStream DeserializeRawSerialStream()
            => new SerialStream(DeserializeRawBytes());

        public SerialStream? DeserializeNullableRawSerialStream()
        {
            var bytes = DeserializeNullableRawBytes();
            return bytes == default ? default : new SerialStream(bytes);
        }

        public bool TryDeserializeSerialStream([NotNullWhen(true)] out SerialStream? value)
        {
            value = default;

            if (!TryDeserializeBytes(out var bytes))
                return false;

            try
            {
                value = new SerialStream(bytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryDeserializeNullableSerialStream(out SerialStream? value)
        {
            value = default;

            if (!TryDeserializeNullableBytes(out var bytes))
                return false;

            if (bytes == default)
                return true;

            try
            {
                value = new SerialStream(bytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public SerialStream ToSerialStream()
        {
            position = 0;
            return DeserializeRawSerialStream();
        }

        public SerialStream? ToNullableSerialStream()
        {
            position = 0;
            return DeserializeNullableRawSerialStream();
        }
    }
}