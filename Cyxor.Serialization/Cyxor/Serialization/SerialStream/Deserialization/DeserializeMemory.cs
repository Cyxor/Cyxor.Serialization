#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0

using System;
using System.Runtime.InteropServices;

namespace Cyxor.Serialization
{
    using Extensions;

    partial class SerializationStream
    {
        public Memory<T> DeserializeMemory<T>() where T: unmanaged
        {
            if (AutoRaw)
                return DeserializeRawMemory<T>();

            var count = DeserializeOp();

            return count == -1 ? throw new InvalidOperationException(Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(typeof(Memory<T>).Name))
                : count == 0 ? Memory<T>.Empty
                : DeserializeMemory<T>(count);
        }

        public Memory<T> DeserializeRawMemory<T>() where T: unmanaged
            => DeserializeMemory<T>(length - position);

        public ref Memory<T> DeserializeRawMemory<T>(ref Memory<T> memory) where T : unmanaged
            => ref DeserializeMemory(ref memory, length - position);

        public Memory<T> DeserializeMemory<T>(int bytesCount) where T : unmanaged
        {
            var memory = new Memory<byte>(new byte[bytesCount]);
            _ = DeserializeMemory(ref memory, bytesCount);
            return memory.Cast<byte, T>();
        }

        public ref Memory<T> DeserializeMemory<T>(ref Memory<T> memory) where T: unmanaged
        {
            var count = DeserializeOp();

            if (count == -1)
                throw new InvalidOperationException
                    (Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingValueType(typeof(Span<T>).Name));

            return ref count == 0 ? ref memory : ref DeserializeMemory(ref memory, count);
        }

        public ref Memory<T> DeserializeMemory<T>(ref Memory<T> memory, int bytesCount) where T: unmanaged
        {
            if (bytesCount < 0)
                throw new ArgumentOutOfRangeException(nameof(bytesCount), $"Parameter {nameof(bytesCount)} must be a positive value");

            if (bytesCount > memory.Length)
                throw new ArgumentOutOfRangeException(nameof(bytesCount));

            if (bytesCount == 0)
                return ref memory;

            EnsureCapacity(bytesCount, SerializerOperation.Deserialize);

            var bufferMemory = new Memory<byte>(buffer, position, bytesCount);
            var memoryOfT = bufferMemory.Cast<byte, T>();

            memoryOfT.CopyTo(memory);
            position += bytesCount;

            return ref memory;
        }

        public bool TryDeserializeMemory<T>(out Memory<T> value) where T : unmanaged
        {
            value = Memory<T>.Empty;

            var currentPosition = position;

            try
            {
                value = DeserializeMemory<T>();
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeMemory<T>(out Memory<T> value, int bytesCount) where T : unmanaged
        {
            value = Memory<T>.Empty;

            if (bytesCount <= 0)
                return false;

            if (length - position < bytesCount)
                return false;

            var currentPosition = position;

            try
            {
                value = DeserializeMemory<T>(bytesCount);
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public Memory<T> ToMemory<T>() where T : unmanaged
        {
            position = 0;
            return DeserializeRawMemory<T>();
        }

        public ReadOnlyMemory<T> DeserializeReadOnlyMemory<T>() where T : unmanaged
        {
            if (AutoRaw)
                return DeserializeRawReadOnlyMemory<T>();

            var count = DeserializeOp();

            return count == -1 ? throw new InvalidOperationException
                (Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingValueType(typeof(ReadOnlyMemory<T>).Name))
                : count == 0 ? ReadOnlyMemory<T>.Empty
                : DeserializeReadOnlyMemory<T>(count);
        }

        public ReadOnlyMemory<T> DeserializeRawReadOnlyMemory<T>() where T : unmanaged
            => DeserializeReadOnlyMemory<T>(length - position);

        public ReadOnlyMemory<T> DeserializeReadOnlyMemory<T>(int bytesCount) where T : unmanaged
        {
            if (bytesCount < 0)
                throw new ArgumentOutOfRangeException(nameof(bytesCount), $"Parameter {nameof(bytesCount)} must be a positive value");

            if (bytesCount == 0)
                return ReadOnlyMemory<T>.Empty;

            EnsureCapacity(bytesCount, SerializerOperation.Deserialize);

            var bufferReadOnlyMemory = new ReadOnlyMemory<byte>(buffer, position, bytesCount);

            position += bytesCount;

            return bufferReadOnlyMemory.Cast<byte, T>();
        }

        public bool TryDeserializeReadOnlyMemory<T>(out ReadOnlyMemory<T> value) where T : unmanaged
        {
            value = ReadOnlyMemory<T>.Empty;

            var currentPosition = position;

            try
            {
                value = DeserializeReadOnlyMemory<T>();
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeReadOnlyMemory<T>(out ReadOnlyMemory<T> value, int bytesCount) where T : unmanaged
        {
            value = ReadOnlyMemory<T>.Empty;

            if (bytesCount <= 0)
                return false;

            if (length - position < bytesCount)
                return false;

            var currentPosition = position;

            try
            {
                value = DeserializeReadOnlyMemory<T>(bytesCount);
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public ReadOnlyMemory<T> ToReadOnlyMemory<T>() where T : unmanaged
        {
            position = 0;
            return DeserializeRawReadOnlyMemory<T>();
        }





        // NULLABLE HERE







        public Memory<T>? DeserializeNullableMemory<T>() where T : unmanaged
        {
            if (AutoRaw)
                return DeserializeNullableRawMemory<T>();

            var count = DeserializeOp();

            return count == -1 ? throw new InvalidOperationException(Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(typeof(Memory<T>).Name))
                : count == 0 ? Memory<T>.Empty
                : DeserializeMemory<T>(count);
        }

        public Memory<T> DeserializeRawMemory<T>() where T : unmanaged
            => DeserializeMemory<T>(length - position);

        public ref Memory<T> DeserializeRawMemory<T>(ref Memory<T> memory) where T : unmanaged
            => ref DeserializeMemory(ref memory, length - position);

        public Memory<T> DeserializeMemory<T>(int bytesCount) where T : unmanaged
        {
            var memory = new Memory<byte>(new byte[bytesCount]);
            _ = DeserializeMemory(ref memory, bytesCount);
            return memory.Cast<byte, T>();
        }

        public ref Memory<T> DeserializeMemory<T>(ref Memory<T> memory) where T : unmanaged
        {
            var count = DeserializeOp();

            if (count == -1)
                throw new InvalidOperationException
                    (Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingValueType(typeof(Span<T>).Name));

            return ref count == 0 ? ref memory : ref DeserializeMemory(ref memory, count);
        }

        public ref Memory<T> DeserializeMemory<T>(ref Memory<T> memory, int bytesCount) where T : unmanaged
        {
            if (bytesCount < 0)
                throw new ArgumentOutOfRangeException(nameof(bytesCount), $"Parameter {nameof(bytesCount)} must be a positive value");

            if (bytesCount > memory.Length)
                throw new ArgumentOutOfRangeException(nameof(bytesCount));

            if (bytesCount == 0)
                return ref memory;

            EnsureCapacity(bytesCount, SerializerOperation.Deserialize);

            var bufferMemory = new Memory<byte>(buffer, position, bytesCount);
            var memoryOfT = bufferMemory.Cast<byte, T>();

            memoryOfT.CopyTo(memory);
            position += bytesCount;

            return ref memory;
        }

        public bool TryDeserializeMemory<T>(out Memory<T> value) where T : unmanaged
        {
            value = Memory<T>.Empty;

            var currentPosition = position;

            try
            {
                value = DeserializeMemory<T>();
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeMemory<T>(out Memory<T> value, int bytesCount) where T : unmanaged
        {
            value = Memory<T>.Empty;

            if (bytesCount <= 0)
                return false;

            if (length - position < bytesCount)
                return false;

            var currentPosition = position;

            try
            {
                value = DeserializeMemory<T>(bytesCount);
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public Memory<T> ToMemory<T>() where T : unmanaged
        {
            position = 0;
            return DeserializeRawMemory<T>();
        }

        public ReadOnlyMemory<T> DeserializeReadOnlyMemory<T>() where T : unmanaged
        {
            if (AutoRaw)
                return DeserializeRawReadOnlyMemory<T>();

            var count = DeserializeOp();

            return count == -1 ? throw new InvalidOperationException
                (Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingValueType(typeof(ReadOnlyMemory<T>).Name))
                : count == 0 ? ReadOnlyMemory<T>.Empty
                : DeserializeReadOnlyMemory<T>(count);
        }

        public ReadOnlyMemory<T> DeserializeRawReadOnlyMemory<T>() where T : unmanaged
            => DeserializeReadOnlyMemory<T>(length - position);

        public ReadOnlyMemory<T> DeserializeReadOnlyMemory<T>(int bytesCount) where T : unmanaged
        {
            if (bytesCount < 0)
                throw new ArgumentOutOfRangeException(nameof(bytesCount), $"Parameter {nameof(bytesCount)} must be a positive value");

            if (bytesCount == 0)
                return ReadOnlyMemory<T>.Empty;

            EnsureCapacity(bytesCount, SerializerOperation.Deserialize);

            var bufferReadOnlyMemory = new ReadOnlyMemory<byte>(buffer, position, bytesCount);

            position += bytesCount;

            return bufferReadOnlyMemory.Cast<byte, T>();
        }

        public bool TryDeserializeReadOnlyMemory<T>(out ReadOnlyMemory<T> value) where T : unmanaged
        {
            value = ReadOnlyMemory<T>.Empty;

            var currentPosition = position;

            try
            {
                value = DeserializeReadOnlyMemory<T>();
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeReadOnlyMemory<T>(out ReadOnlyMemory<T> value, int bytesCount) where T : unmanaged
        {
            value = ReadOnlyMemory<T>.Empty;

            if (bytesCount <= 0)
                return false;

            if (length - position < bytesCount)
                return false;

            var currentPosition = position;

            try
            {
                value = DeserializeReadOnlyMemory<T>(bytesCount);
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public ReadOnlyMemory<T> ToReadOnlyMemory<T>() where T : unmanaged
        {
            position = 0;
            return DeserializeRawReadOnlyMemory<T>();
        }






        /*
                public Memory<T>? DeserializeNullableMemory<T>()
                {
                    if (AutoRaw)
                        return DeserializeNullableRawMemory();

                    var count = DeserializeOp();

                    return count == -1 ? default
                        : count == 0 ? Memory<T>.Empty
                        : DeserializeNullableMemory(count);
                }



                public Memory<T>? ToNullableMemory<T>()
                {
                    position = 0;
                    return DeserializeNullableRawMemory<T>();
                }



                public Memory<T>? DeserializeNullableRawMemory<T>()
                    => DeserializeNullableMemory(length - position);

                public Memory<T>? DeserializeNullableMemory<T>(int count)
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



                //unsafe void DeserializeBytes(byte* destination, int destinationSize, int bytesToCopy, bool zeroBytesToCopy)
                //{
                //    if ((IntPtr)destination == IntPtr.Zero)
                //        throw new ArgumentNullException(nameof(destination));

                //    if (destinationSize < 0)
                //        throw new ArgumentOutOfRangeException(nameof(destinationSize), $"{nameof(destinationSize)} must be a positive value");

                //    if (bytesToCopy < 0)
                //        throw new ArgumentOutOfRangeException(nameof(bytesToCopy), $"{nameof(bytesToCopy)} must be a positive value");

                //    if (bytesToCopy == 0)
                //    {
                //        if (!zeroBytesToCopy)
                //            throw new ArgumentOutOfRangeException(nameof(bytesToCopy), $"{nameof(bytesToCopy)} must be greater than zero. To read the length from data use an overload.");

                //        bytesToCopy = DeserializeOp();

                //        if (bytesToCopy <= 0)
                //            return;
                //    }

                //    if (destinationSize - bytesToCopy < 0)
                //        throw new ArgumentOutOfRangeException(nameof(bytesToCopy), $"{nameof(bytesToCopy)} is greater than {nameof(destinationSize)}");

                //    EnsureCapacity(bytesToCopy, SerializerOperation.Deserialize);

                //    fixed (byte* src = buffer)
                //        Utilities.Memory.Memcpy(src + position, destination, bytesToCopy);

                //    position += bytesToCopy;
                //}

                */
    }
}

#endif