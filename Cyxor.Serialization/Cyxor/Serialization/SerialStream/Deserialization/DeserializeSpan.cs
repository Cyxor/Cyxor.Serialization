#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0

using System;
using System.Diagnostics.CodeAnalysis;

namespace Cyxor.Serialization
{
    using Extensions;

    partial class SerializationStream
    {
        public Span<T> DeserializeSpan<T>() where T: struct
        {
            if (AutoRaw)
                return DeserializeRawSpan<T>();

            var count = DeserializeOp();

            return count == -1 ? throw new InvalidOperationException(Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingValueType(typeof(Span<T>).Name))
                : count == 0 ? Span<T>.Empty
                : DeserializeSpan<T>(count);
        }

        public Span<T> DeserializeRawSpan<T>() where T : struct
            => DeserializeSpan<T>(length - position);

        public ref Span<T> DeserializeRawSpan<T>(ref Span<T> span) where T : struct
            => ref DeserializeSpan(ref span, length - position);

        public Span<T> DeserializeSpan<T>(int bytesCount) where T: struct
        {
            var span = new Span<byte>(new byte[bytesCount]);
            _ = DeserializeSpan(ref span, bytesCount);
            return span.ToSpanOf<T>();
        }

        public ref Span<T> DeserializeSpan<T>(ref Span<T> span) where T: struct
        {
            var count = DeserializeOp();

            if (count == -1)
                throw new InvalidOperationException(Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingValueType(typeof(Span<T>).Name));

            return ref count == 0 ? ref span : ref DeserializeSpan(ref span, count);
        }

        public ref Span<T> DeserializeSpan<T>(ref Span<T> span, int bytesCount) where T: struct
        {
            if (bytesCount < 0)
                throw new ArgumentOutOfRangeException(nameof(bytesCount), $"Parameter {nameof(bytesCount)} must be a positive value");

            if (bytesCount > span.Length)
                throw new ArgumentOutOfRangeException(nameof(bytesCount));

            if (bytesCount == 0)
                return ref span;

            EnsureCapacity(bytesCount, SerializerOperation.Deserialize);

            var bufferSpan = new Span<byte>(buffer, position, bytesCount);
            var spanOfT = bufferSpan.ToSpanOf<T>();

            spanOfT.CopyTo(span);
            position += bytesCount;

            return ref span;
        }

        public bool TryDeserializeSpan<T>(out Span<T> value) where T: struct
        {
            value = Span<T>.Empty;

            var currentPosition = position;

            try
            {
                value = DeserializeSpan<T>();
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeSpan<T>(out Span<T> value, int count) where T: struct
        {
            value = Span<T>.Empty;

            if (count <= 0)
                return false;

            if (length - position < count)
                return false;

            var currentPosition = position;

            try
            {
                value = DeserializeSpan<T>(count);
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public Span<T> ToSpan<T>() where T : struct
        {
            position = 0;
            return DeserializeRawSpan<T>();
        }

        public ReadOnlySpan<T> DeserializeReadOnlySpan<T>() where T : struct
        {
            if (AutoRaw)
                return DeserializeRawReadOnlySpan<T>();

            var count = DeserializeOp();

            return count == -1 ? throw new InvalidOperationException(Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingValueType(typeof(ReadOnlySpan<T>).Name))
                : count == 0 ? ReadOnlySpan<T>.Empty
                : DeserializeReadOnlySpan<T>(count);
        }

        public ReadOnlySpan<T> DeserializeRawReadOnlySpan<T>() where T : struct
            => DeserializeReadOnlySpan<T>(length - position);

        public ReadOnlySpan<T> DeserializeReadOnlySpan<T>(int bytesCount) where T : struct
        {
            if (bytesCount < 0)
                throw new ArgumentOutOfRangeException(nameof(bytesCount), $"Parameter {nameof(bytesCount)} must be a positive value");

            if (bytesCount == 0)
                return ReadOnlySpan<T>.Empty;

            EnsureCapacity(bytesCount, SerializerOperation.Deserialize);

            var bufferReadOnlySpan = new ReadOnlySpan<byte>(buffer, position, bytesCount);

            position += bytesCount;

            return bufferReadOnlySpan.ToReadOnlySpanOf<T>();
        }

        public bool TryDeserializeReadOnlySpan<T>(out ReadOnlySpan<T> value) where T : struct
        {
            value = ReadOnlySpan<T>.Empty;

            var currentPosition = position;

            try
            {
                value = DeserializeReadOnlySpan<T>();
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeReadOnlySpan<T>(out ReadOnlySpan<T> value, int count) where T : struct
        {
            value = ReadOnlySpan<T>.Empty;

            if (count <= 0)
                return false;

            if (length - position < count)
                return false;

            var currentPosition = position;

            try
            {
                value = DeserializeReadOnlySpan<T>(count);
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public ReadOnlySpan<T> ToReadOnlySpan<T>() where T : struct
        {
            position = 0;
            return DeserializeRawReadOnlySpan<T>();
        }
    }
}

#endif