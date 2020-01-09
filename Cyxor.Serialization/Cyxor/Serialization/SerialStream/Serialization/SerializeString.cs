using System;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        void InternalSerialize(string? value, bool raw)
        {
            if (value == null)
            {
                if (!raw)
                    Serialize((byte)0);

                return;
            }

            //unsafe
            {
                //fixed (char* ptr = value)
                    //InternalSerialize(ptr, value!.Length, 0, value!.Length, false, false, raw);
                    //InternalSerialize(ptr, value!.Length, 0, value!.Length, true, false, raw);
            }

            var myLength = value.Length;
            SerializeOp(myLength == 0 ? ObjectProperties.EmptyMap : myLength);

            EnsureCapacity(myLength);

            var result = System.Text.Unicode.Utf8.FromUtf16(value.AsSpan(), buffer.AsSpan(position, Capacity - position), out var charsRead, out var bytesWritten);

            if (result != System.Buffers.OperationStatus.Done)
                throw new InvalidOperationException("not done");

            position += bytesWritten;
            length = position;
        }

        public void Serialize(string? value)
            => InternalSerialize(value, AutoRaw);

        public void SerializeRaw(string? value)
            => InternalSerialize(value, raw: true);
    }
}