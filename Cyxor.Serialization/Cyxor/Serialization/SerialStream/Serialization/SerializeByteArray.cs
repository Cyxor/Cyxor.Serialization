using System;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        public void Serialize(byte[]? value)
            => InternalSerialize(value, 0, value?.Length ?? 0, raw: AutoRaw);

        public void Serialize(byte[]? value, int index, int count)
            => InternalSerialize(value, index, count, raw: false);

        public void SerializeRaw(byte[]? value)
            => InternalSerialize(value, 0, value?.Length ?? 0, raw: true);

        public void SerializeRaw(byte[]? value, int index, int count)
            => InternalSerialize(value, index, count, raw: true);

        public unsafe void Serialize(byte* value)
            => InternalSerialize(value, 0, 0, 0, true, raw: AutoRaw);

        unsafe public void Serialize(byte* value, int count)
            => InternalSerialize(value, count, 0, count, false, raw: false);

        public unsafe void SerializeRaw(byte* value)
            => InternalSerialize(value, 0, 0, 0, true, raw: true);

        public unsafe void SerializeRaw(byte* value, int count)
            => InternalSerialize(value, count, 0, count, false, raw: true);

        unsafe void InternalSerialize(byte[]? value, int index, int count, bool raw)
        {
            if (value == default)
                InternalSerialize((byte*)IntPtr.Zero, 0, index, count, calculateLength: false, raw: raw);
            else
                fixed (byte* ptr = value)
                    InternalSerialize(ptr, value.Length, index, count, calculateLength: false, raw: raw);
        }

        unsafe void InternalSerialize(byte* value, int size, int index, int count, bool calculateLength, bool raw)
        {
            if (calculateLength)
                if ((IntPtr)value != IntPtr.Zero)
                    size = count = Utilities.Memory.Strlen(value);

            if ((IntPtr)value == IntPtr.Zero)
                if (raw && size == 0 && index == 0 && count == 0)
                    return;
                else if (raw || size != 0 || index != 0 || count != 0)
                    throw new ArgumentNullException(nameof(value));
                else
                {
                    Serialize((byte)0);
                    return;
                }

            if (index < 0 || count < 0 || size < 0)
                throw new ArgumentOutOfRangeException($"{nameof(index)}, {nameof(count)} or {nameof(size)}");

            if (size - index < count)
                throw new ArgumentException($"{nameof(size)} - {nameof(index)} < {nameof(count)}");

            if (count == 0)
            {
                if (!raw)
                    Serialize(ObjectProperties.EmptyMap);

                return;
            }

            var varIntSize = 0;

            if (!raw)
                varIntSize = Utilities.EncodedInteger.RequiredBytes((uint)count);

            if (position + count + varIntSize < 0)
                throw new InvalidOperationException("Buffer too long.");

            if (!raw)
                SerializeOp(count);

            EnsureCapacity(count, SerializerOperation.Serialize);

            fixed (byte* ptr = buffer)
                Utilities.Memory.Memcpy(value + index, ptr + position, count);

            position += count;
        }
    }
}