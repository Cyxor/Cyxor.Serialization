#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0

using System;

namespace Cyxor.Serialization
{
    using Extensions;

    partial class SerializationStream
    {
        void InternalSerialize<T>(in ReadOnlySpan<T> readOnlySpan, bool raw) where T : struct
        {
            if (readOnlySpan.IsEmpty)
            {
                if (!raw)
                    Serialize(ObjectProperties.EmptyMap);

                return;
            }

            var bytesReadOnlySpan = readOnlySpan.ToReadOnlySpanOfBytes();

            if (!raw)
                SerializeOp(bytesReadOnlySpan.Length);

            EnsureCapacity(bytesReadOnlySpan.Length, SerializerOperation.Serialize);

            bytesReadOnlySpan.CopyTo(new Span<byte>(buffer, position, bytesReadOnlySpan.Length));
        }

        public void Serialize<T>(Span<T> span) where T : struct
            => InternalSerialize((ReadOnlySpan<T>)span, AutoRaw);

        public void SerializeRaw<T>(Span<T> span) where T : struct
            => InternalSerialize((ReadOnlySpan<T>)span, raw: true);

        public void Serialize<T>(ReadOnlySpan<T> readOnlySpan) where T : struct
            => InternalSerialize(readOnlySpan, AutoRaw);

        public void SerializeRaw<T>(ReadOnlySpan<T> readOnlySpan) where T : struct
            => InternalSerialize(readOnlySpan, raw: true);
    }
}

#endif