using System;

namespace Cyxor.Serialization
{
    partial class SerialStream
    {
        public void Serialize(char[]? value)
            => InternalSerialize(value, 0, value?.Length ?? 0, wide: false, raw: AutoRaw);

        public void Serialize(char[]? value, int index, int count)
            => InternalSerialize(value, index, count, wide: false, raw: false);

        public void SerializeRaw(char[]? value)
            => InternalSerialize(value, 0, value?.Length ?? 0, wide: false, raw: true);

        public void SerializeRaw(char[]? value, int index, int count)
            => InternalSerialize(value, index, count, wide: false, raw: true);

        public unsafe void Serialize(char* value)
            => InternalSerialize(value, 0, 0, 0, wide: false, calculateLength: true, raw: AutoRaw);

        public unsafe void Serialize(char* value, int count)
            => InternalSerialize(value, count, 0, count, wide: false, calculateLength: false, raw: false);

        public unsafe void SerializeRaw(char* value)
            => InternalSerialize(value, 0, 0, 0, wide: false, calculateLength: true, raw: true);

        public unsafe void SerializeRaw(char* value, int count)
            => InternalSerialize(value, count, 0, count, wide: false, calculateLength: false, raw: true);

        unsafe void InternalSerialize(char[]? value, int index, int count, bool wide, bool raw)
        {
            if (value == default)
                InternalSerialize((char*)IntPtr.Zero, 0, index, count, wide, false, raw);
            else
                fixed (char* ptr = value)
                    InternalSerialize(ptr, value.Length, index, count, wide, false, raw);
        }

        unsafe void InternalSerialize(char* value, int size, int index, int count, bool wide, bool calculateLength, bool raw)
        {
            if (calculateLength)
                if ((IntPtr)value != IntPtr.Zero)
                    size = count = Utilities.Memory.Wcslen(value);

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
            var byteCount = count * 2;

            if (wide)
            {
                if (!raw)
                    varIntSize = Utilities.EncodedInteger.RequiredBytes((uint)byteCount);

                if (position + byteCount + varIntSize < 0)
                    throw new InvalidOperationException("Buffer too long.");

                if (!raw)
                    SerializeOp(byteCount);

                EnsureCapacity(byteCount, SerializerOperation.Serialize);

                fixed (byte* ptr = buffer)
                    Utilities.Memory.Wstrcpy(value + index, (char*)(ptr + position), count);

                position += byteCount;

                return;
            }

#if !NETSTANDARD1_0
            byteCount = Encoding.GetByteCount(value + index, count);
#else
            var managedValue = new char[count];
            fixed (char* charPtr = managedValue)
                Utilities.Memory.Wstrcpy(value + index, charPtr, count);
            byteCount = Encoding.GetByteCount(managedValue);
#endif

            if (!raw)
                varIntSize = Utilities.EncodedInteger.RequiredBytes((uint)byteCount);

            if (position + byteCount + varIntSize < 0)
                throw new InvalidOperationException("Buffer too long.");

            if (!raw)
                SerializeOp(byteCount);

            var previousLength = length;

            EnsureCapacity(byteCount, SerializerOperation.Serialize);

            var realByteCount = 0;

#if !NETSTANDARD1_0
            fixed (byte* ptr = buffer)
                realByteCount = Encoding.GetBytes(value + index, count, ptr + position, byteCount);
#else
            realByteCount = Encoding.GetBytes(managedValue, 0, count, buffer, position);
#endif

            if (!raw)
                if (realByteCount != byteCount)
                {
                    var realVarIntSize = Utilities.EncodedInteger.RequiredBytes((uint)realByteCount);

                    position -= varIntSize;
                    SerializeOp(realByteCount);

                    var diff = varIntSize - realVarIntSize;

                    if (diff > 0)
                        fixed (byte* ptr = &buffer![position + diff])
                            Utilities.Memory.Memcpy(ptr, ptr - diff, realByteCount);
                }

            position += realByteCount;

            if (position > previousLength)
                length = position;
        }
    }
}