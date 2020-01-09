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

#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0
using System.Buffers;
#endif

namespace Cyxor.Serialization
{
    using System.Xml.Schema;
    using Extensions;

    partial class Serializer
    {
        public Serializer() { }

        public Serializer(SerializerOptions options)
        {
            Options = options;
        }

        public Serializer(byte[] buffer, SerializerOptions options = default) : this(options)
            => SetBuffer(buffer);

        public Serializer(Stream stream, SerializerOptions options = default) : this(options)
            => SerializeRaw(stream);

        public Serializer(ArraySegment<byte> arraySegment, SerializerOptions options = default) : this(options)
            => SetBuffer(arraySegment);

        public Serializer(byte[] buffer, int offset, int count, SerializerOptions options = default) : this(options)
            => SetBuffer(buffer, offset, count);

        public Serializer(Memory<byte> memory, SerializerOptions options = default) : this(options)
            => SerializeRaw(memory);

        public Serializer(ReadOnlyMemory<byte> readOnlyMemory, SerializerOptions options = default)
        {
            if (options == default)
                options = new SerializerOptions(readOnly: true);

            if (!options.ReadOnly)
                throw new ArgumentException($"The serializer {nameof(options)} must be equal to default or the {nameof(SerializerOptions)}.{nameof(SerializerOptions.ReadOnly)} field must be set to true", nameof(options));

            Options = options;

            // TODO:
        }

        public Serializer(object value)
            => SerializeRaw(value);

        public Serializer(object value, IBackingSerializer backingSerializer, object? backingSerializerOptions = default)
            => SerializeRaw(value, backingSerializer, backingSerializerOptions);
    }
}