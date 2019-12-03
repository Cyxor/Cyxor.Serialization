﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace Cyxor.Serialization
{
    partial class SerializationStream
    {
        public byte[] DeserializeBytes()
        {
            if (AutoRaw)
                return DeserializeRawBytes();

            var count = DeserializeOp();

            return count == -1 ? throw new InvalidOperationException(Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(typeof(byte[]).Name))
                : count == 0 ? Utilities.Array.Empty<byte>()
                : DeserializeBytes(count);
        }

        public byte[]? DeserializeNullableBytes()
        {
            if (AutoRaw)
                return DeserializeNullableRawBytes();

            var count = DeserializeOp();

            return count == -1 ? default
                : count == 0 ? Utilities.Array.Empty<byte>()
                : DeserializeNullableBytes(count);
        }

        public byte[] DeserializeRawBytes()
            => DeserializeBytes(length - position);

        public byte[]? DeserializeNullableRawBytes()
            => DeserializeNullableBytes(length - position);

        public byte[] DeserializeBytes(int count)
        {
            if (count == 0)
                return Utilities.Array.Empty<byte>();

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), $"Parameter {nameof(count)} must be a positive value");

            EnsureCapacity(count, SerializerOperation.Deserialize);

            var value = new byte[count];

            unsafe
            {
                fixed (byte* src = buffer, dest = value)
                    Utilities.Memory.Memcpy(src + position, dest, count);
            }

            position += count;
            return value;
        }

        public byte[]? DeserializeNullableBytes(int count)
        {
            if (count == 0)
                return default;

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), $"Parameter {nameof(count)} must be a positive value");

            EnsureCapacity(count, SerializerOperation.Deserialize);

            var value = new byte[count];

            unsafe
            {
                fixed (byte* src = buffer, dest = value)
                    Utilities.Memory.Memcpy(src + position, dest, count);
            }

            position += count;
            return value;
        }

        public void DeserializeBytes(byte[] dest, int offset = 0)
        {
            unsafe
            {
                fixed (byte* ptr = dest)
                    DeserializeBytes(ptr + offset, dest.Length - offset, 0, zeroBytesToCopy: true);
            }
        }

        public void DeserializeBytes(byte[] dest, int offset, int count)
        {
            unsafe
            {
                fixed (byte* ptr = dest)
                    DeserializeBytes(ptr + offset, dest.Length - offset, count, zeroBytesToCopy: false);
            }
        }

        public unsafe void DeserializeBytes(byte* destination, int destinationSize)
            => DeserializeBytes(destination, destinationSize, 0, zeroBytesToCopy: true);

        public unsafe void DeserializeBytes(byte* destination, int destinationSize, int bytesToCopy)
            => DeserializeBytes(destination, destinationSize, bytesToCopy, zeroBytesToCopy: false);

        unsafe void DeserializeBytes(byte* destination, int destinationSize, int bytesToCopy, bool zeroBytesToCopy)
        {
            if ((IntPtr)destination == IntPtr.Zero)
                throw new ArgumentNullException(nameof(destination));

            if (destinationSize < 0)
                throw new ArgumentOutOfRangeException(nameof(destinationSize), $"{nameof(destinationSize)} must be a positive value");

            if (bytesToCopy < 0)
                throw new ArgumentOutOfRangeException(nameof(bytesToCopy), $"{nameof(bytesToCopy)} must be a positive value");

            if (bytesToCopy == 0)
            {
                if (!zeroBytesToCopy)
                    throw new ArgumentOutOfRangeException(nameof(bytesToCopy), $"{nameof(bytesToCopy)} must be greater than zero. To read the length from data use an overload.");

                bytesToCopy = DeserializeOp();

                if (bytesToCopy <= 0)
                    return;
            }

            if (destinationSize - bytesToCopy < 0)
                throw new ArgumentOutOfRangeException(nameof(bytesToCopy), $"{nameof(bytesToCopy)} is greater than {nameof(destinationSize)}");

            EnsureCapacity(bytesToCopy, SerializerOperation.Deserialize);

            fixed (byte* src = buffer)
                Utilities.Memory.Memcpy(src + position, destination, bytesToCopy);

            position += bytesToCopy;
        }

        public bool TryDeserializeBytes([NotNullWhen(true)] out byte[]? value)
        {
            value = default;

            var currentPosition = position;

            try
            {
                value = DeserializeNullableBytes();

                if (value == default)
                {
                    position -= 1;
                    return false;
                }

                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeNullableBytes(out byte[]? value)
        {
            value = default;

            var currentPosition = position;

            try
            {
                value = DeserializeNullableBytes();
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeBytes([NotNullWhen(true)] out byte[]? value, int count)
        {
            value = default;

            if (count <= 0)
                return false;

            if (length - position < count)
                return false;

            var currentPosition = position;

            try
            {
                value = DeserializeBytes(count);
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeNullableBytes(out byte[]? value, int count)
        {
            value = default;

            if (count <= 0)
                return false;

            if (length - position < count)
                return false;

            var currentPosition = position;

            try
            {
                value = DeserializeNullableBytes(count);
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public byte[] ToByteArray()
        {
            position = 0;
            return DeserializeRawBytes();
        }

        public byte[]? ToNullableBytes()
        {
            position = 0;
            return DeserializeNullableRawBytes();
        }
    }
}