#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0

using System;
using System.Diagnostics.CodeAnalysis;

namespace Cyxor.Serialization
{
    //partial class SerialStream
    //{
    //    public Memory<T> ToMemory<T>() where T : struct
    //    {
    //        position = 0;
    //        return DeserializeRawMemory<T>();
    //    }

    //    public Memory<T>? ToNullableMemory<T>()
    //    {
    //        position = 0;
    //        return DeserializeNullableRawMemory<T>();
    //    }

    //    public Memory<T> DeserializeMemory<T>()
    //    {
    //        if (AutoRaw)
    //            return DeserializeRawMemory();

    //        var count = DeserializeOp();

    //        return count == -1 ? throw new InvalidOperationException(Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(typeof(Memory<T>).Name))
    //            : count == 0 ? Utilities.Array.Empty<byte>()
    //            : DeserializeMemory(count);
    //    }

    //    public Memory<T>? DeserializeNullableMemory<T>()
    //    {
    //        if (AutoRaw)
    //            return DeserializeNullableRawMemory();

    //        var count = DeserializeOp();

    //        return count == -1 ? default
    //            : count == 0 ? Memory<T>.Empty
    //            : DeserializeNullableMemory(count);
    //    }

    //    public Memory<T> DeserializeRawMemory<T>()
    //        => DeserializeBytes(length - position);

    //    public Memory<T>? DeserializeNullableRawMemory<T>()
    //        => DeserializeNullableMemory(length - position);

    //    public Memory<T> DeserializeMemory<T>(int count)
    //    {
    //        if (count == 0)
    //            return Memory<T>.Empty;

    //        if (count < 0)
    //            throw new ArgumentOutOfRangeException(nameof(count), $"Parameter {nameof(count)} must be a positive value");

    //        EnsureCapacity(count, SerializerOperation.Deserialize);

    //        var value = new byte[count];

    //        unsafe
    //        {
    //            fixed (byte* src = buffer, dest = value)
    //                Utilities.Memory.Memcpy(src + position, dest, count);
    //        }

    //        position += count;
    //        return value;
    //    }

    //    public Memory<T>? DeserializeNullableMemory<T>(int count)
    //    {
    //        if (count == 0)
    //            return default;

    //        if (count < 0)
    //            throw new ArgumentOutOfRangeException(nameof(count), $"Parameter {nameof(count)} must be a positive value");

    //        EnsureCapacity(count, SerializerOperation.Deserialize);

    //        var value = new byte[count];

    //        unsafe
    //        {
    //            fixed (byte* src = buffer, dest = value)
    //                Utilities.Memory.Memcpy(src + position, dest, count);
    //        }

    //        position += count;
    //        return value;
    //    }

    //    public void DeserializeMemory<T>(ref Memory<T> dest)
    //    {
    //        unsafe
    //        {
    //            fixed (byte* ptr = dest)
    //                DeserializeMemory(ptr, dest.Length, 0, zeroBytesToCopy: true);
    //        }
    //    }

    //    public void DeserializeMemory<T>(ref Memory<T> dest, int count)
    //    {
    //        unsafe
    //        {
    //            fixed (byte* ptr = dest)
    //                DeserializeBytes(ptr, dest.Length, count, zeroBytesToCopy: false);
    //        }
    //    }

    //    //unsafe void DeserializeBytes(byte* destination, int destinationSize, int bytesToCopy, bool zeroBytesToCopy)
    //    //{
    //    //    if ((IntPtr)destination == IntPtr.Zero)
    //    //        throw new ArgumentNullException(nameof(destination));

    //    //    if (destinationSize < 0)
    //    //        throw new ArgumentOutOfRangeException(nameof(destinationSize), $"{nameof(destinationSize)} must be a positive value");

    //    //    if (bytesToCopy < 0)
    //    //        throw new ArgumentOutOfRangeException(nameof(bytesToCopy), $"{nameof(bytesToCopy)} must be a positive value");

    //    //    if (bytesToCopy == 0)
    //    //    {
    //    //        if (!zeroBytesToCopy)
    //    //            throw new ArgumentOutOfRangeException(nameof(bytesToCopy), $"{nameof(bytesToCopy)} must be greater than zero. To read the length from data use an overload.");

    //    //        bytesToCopy = DeserializeOp();

    //    //        if (bytesToCopy <= 0)
    //    //            return;
    //    //    }

    //    //    if (destinationSize - bytesToCopy < 0)
    //    //        throw new ArgumentOutOfRangeException(nameof(bytesToCopy), $"{nameof(bytesToCopy)} is greater than {nameof(destinationSize)}");

    //    //    EnsureCapacity(bytesToCopy, SerializerOperation.Deserialize);

    //    //    fixed (byte* src = buffer)
    //    //        Utilities.Memory.Memcpy(src + position, destination, bytesToCopy);

    //    //    position += bytesToCopy;
    //    //}
    //}
}

#endif