using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization
{
    using Extensions;

    partial class Serializer
    {
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;

        public override long Position
        {
            get => position;
            set
            {
                if (value < 0 || value > length)
                    throw new ArgumentOutOfRangeException(nameof(value));

                position = (int)value;
            }
        }

        public override long Length => length;

        public override int ReadByte()
            => DeserializeByte();

        public override void WriteByte(byte value)
            => Serialize(value);

        public override int Read(byte[] buffer, int offset, int count)
        {
            count = count > length - position ? length - position : count;
            DeserializeBytes(buffer, offset, count);
            return count;
        }

        public override int Read(Span<byte> buffer)
        {
            var initialPosition = position;
            _ = DeserializeRawSpan(ref buffer);
            return position - initialPosition;
        }

        public override void Write(byte[] buffer, int offset, int count)
            => SerializeRaw(buffer, offset, count);

        public override void Write(ReadOnlySpan<byte> buffer)
            => SerializeRaw(buffer);

        public override void Flush() { }

        public override void SetLength(long value)
            => SetLength((int)value);

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin: Position = offset; break;
                case SeekOrigin.End: Position = length - offset; break;
                case SeekOrigin.Current: Position = position + offset; break;
            }

            return position;
        }
    }
}