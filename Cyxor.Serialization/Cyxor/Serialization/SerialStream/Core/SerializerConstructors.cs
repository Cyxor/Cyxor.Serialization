using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Buffers;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

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