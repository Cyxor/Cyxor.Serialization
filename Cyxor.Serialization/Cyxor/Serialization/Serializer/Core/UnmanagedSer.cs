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
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization
{
    using Extensions;

    using MethodDictionary = Dictionary<Type, MethodInfo>;
    using BufferOverflowException = EndOfStreamException;
    using System.Diagnostics.CodeAnalysis;
    using System.ComponentModel;

    partial class Serializer
    {
        delegate void SerializeUnmanagedX<T>(T value);

        static class UnmanagedSer<T>
        {
            static Type ArgumentType = typeof(T);

            static bool IsKnownUnmanaged()
                => RuntimeHelpers.IsReferenceOrContainsReferences<T>()
                    ? false
                    : ArgumentType.IsPrimitive ||
                    ArgumentType == typeof(decimal) ||
                    ArgumentType == typeof(Guid) ||
                    ArgumentType == typeof(DateTime);

            static UnmanagedSer()
            {
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                    if (!IsKnownUnmanaged())

            }

            public static readonly SerializeUnmanagedX<T> SerializeUnmanagedDelegate = (SerializeUnmanaged<T>)SerializeUnmanagedInfo.Method.CreateDelegate(typeof(SerializeUnmanaged<T>));
        }



        public static bool IsKnownType(Type type)
        {
            if (KnownTypesCache.TryGetValue(type, out var result))
                return result;

            result =
                type.IsPrimitive ||
                type.IsEnum ||
                type.IsArray ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(Guid) ||
                type == typeof(TimeSpan) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(BitSerializer) ||
                type == typeof(Uri) ||
                type == typeof(Serializer) ||
                type == typeof(MemoryStream) ||
                type.IsInterfaceImplemented<IEnumerable>();

            _ = KnownTypesCache.TryAdd(type, result);

            return result;
        }
    }
}