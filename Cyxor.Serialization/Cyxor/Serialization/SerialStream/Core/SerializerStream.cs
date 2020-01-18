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
            get => _position;
            set
            {
                if (value < 0 || value > _length)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _position = (int)value;
            }
        }

        public override long Length => _length;

        public override int ReadByte()
            => DeserializeByte();

        public override void WriteByte(byte value)
            => Serialize(value);

        public override int Read(byte[] buffer, int offset, int count)
        {
            count = count > _length - _position ? _length - _position : count;
            DeserializeBytes(buffer, offset, count);
            return count;
        }

        public override int Read(Span<byte> buffer)
        {
            var initialPosition = _position;
            _ = DeserializeRawSpan(ref buffer);
            return _position - initialPosition;
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
                case SeekOrigin.End: Position = _length - offset; break;
                case SeekOrigin.Current: Position = _position + offset; break;
            }

            return _position;
        }
    }
}