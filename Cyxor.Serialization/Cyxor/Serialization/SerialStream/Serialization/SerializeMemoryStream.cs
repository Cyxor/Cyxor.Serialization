using System.IO;

namespace Cyxor.Serialization
{
#if NETSTANDARD1_0 || NETSTANDARD1_3
    using Extensions;
#endif

    using BufferOverflowException = EndOfStreamException;

    partial class SerialStream
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

            InternalSerialize(value.GetBuffer(), 0, (int)value.Length, raw);
        }

        public void Serialize(MemoryStream? value)
            => InternalSerialize(value, AutoRaw);

        public void SerializeRaw(MemoryStream? value)
            => InternalSerialize(value, raw: true);
    }
}