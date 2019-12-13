using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

#if !NET20
using System.Linq.Expressions;
#endif

#if !NET20 && !NET35 && !NET40
using System.Runtime.CompilerServices;
#endif

#if !NET20 && !NET35 && !NETSTANDARD1_0
using System.Collections.Concurrent;
#else
using System.Threading;
#endif

namespace Cyxor.Serialization
{
    using Extensions;

    partial class SerializationStream
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
            DeserializeBytes(buffer, offset, count);
            return position == length ? 0 : count;
        }

        public bool StreamWriteRaw { get; set; }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!StreamWriteRaw)
                Serialize(buffer, offset, count);
            else
                SerializeRaw(buffer, offset, count);
        }

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