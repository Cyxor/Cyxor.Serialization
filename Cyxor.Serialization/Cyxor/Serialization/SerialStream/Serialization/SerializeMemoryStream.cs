using System;
using System.IO;
using System.Threading.Tasks;

namespace Cyxor.Serialization
{
    using BufferOverflowException = EndOfStreamException;

    partial class Serializer
    {
        void InternalSerialize(MemoryStream? value, bool raw)
        {
            if (value == default)
            {
                if (!raw)
                    Serialize((byte)0);

                return;
            }

            if (value.Length > int.MaxValue)
                throw new BufferOverflowException();

            if (value.TryGetBuffer(out var arraySegment))
                InternalSerialize(arraySegment.Array, arraySegment.Offset, arraySegment.Count, raw);
            else
            {
                // TODO: Resort to stream.CopyTo(...) Implement Stream
                throw new NotImplementedException();
            }
        }

        public void Serialize(MemoryStream? value)
            => InternalSerialize(value, AutoRaw);

        public void SerializeRaw(MemoryStream? value)
            => InternalSerialize(value, raw: true);
    }
}