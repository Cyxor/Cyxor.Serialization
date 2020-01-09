using System.IO;
using System.Diagnostics.CodeAnalysis;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        public MemoryStream DeserializeMemoryStream()
            => new MemoryStream(DeserializeBytes());

        public MemoryStream? DeserializeNullableMemoryStream()
        {
            var bytes = DeserializeNullableBytes();
            return bytes == default ? default : new MemoryStream(bytes);
        }

        public MemoryStream DeserializeRawMemoryStream()
            => new MemoryStream(DeserializeRawBytes());

        public MemoryStream? DeserializeNullableRawMemoryStream()
        {
            var bytes = DeserializeNullableRawBytes();
            return bytes == default ? default : new MemoryStream(bytes);
        }

        public bool TryDeserializeMemoryStream([NotNullWhen(true)] out MemoryStream? value)
        {
            value = default;

            if (!TryDeserializeBytes(out var bytes))
                return false;

            try
            {
                value = new MemoryStream(bytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryDeserializeNullableMemoryStream(out MemoryStream? value)
        {
            value = default;

            if (!TryDeserializeNullableBytes(out var bytes))
                return false;

            if (bytes == default)
                return true;

            try
            {
                value = new MemoryStream(bytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public MemoryStream ToMemoryStream()
        {
            position = 0;
            return DeserializeRawMemoryStream();
        }

        public MemoryStream? ToNullableMemoryStream()
        {
            position = 0;
            return DeserializeNullableRawMemoryStream();
        }
    }
}