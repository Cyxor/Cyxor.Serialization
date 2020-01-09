namespace Cyxor.Serialization
{
    partial class Serializer
    {
        public void SerializeCompressedInt(short value)
            => SerializeCompressedInt((ulong)((value << 1) ^ (value >> 15)));

        public void SerializeCompressedInt(int value)
            => SerializeCompressedInt((ulong)((value << 1) ^ (value >> 31)));

        public void SerializeCompressedInt(long value)
            => SerializeCompressedInt((ulong)((value << 1) ^ (value >> 63)));

        public void SerializeCompressedInt(ushort value)
            => SerializeCompressedInt((ulong)value);

        public void SerializeCompressedInt(uint value)
            => SerializeCompressedInt((ulong)value);

        public void SerializeCompressedInt(ulong value)
        {
            while (value >= 128)
            {
                Serialize((byte)(value | 128));
                value >>= 7;
            }

            Serialize((byte)value);
        }
    }
}