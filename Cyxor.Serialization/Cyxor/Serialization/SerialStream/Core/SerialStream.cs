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
    using Extensions;

    using MethodDictionary = Dictionary<Type, MethodInfo>;
    using BufferOverflowException = EndOfStreamException;

    [DebuggerDisplay("{DebuggerDisplay()}")]
    public sealed partial class SerializationStream : Stream
    {
        bool AutoRaw;

        #region Static

#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0
        static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Create();
#endif

        //static readonly Type NullableGenericType = typeof(SerialStream);

        static readonly MethodDictionary SerializeMethods =
            (from method in typeof(SerializationStream)
               .GetMethodsInfo(name: nameof(SerializerOperation.Serialize), publicMethods: true, parametersCount: 1)
             let parameterType = method.GetParameters().Single().ParameterType
             where !parameterType.IsPointer
             orderby parameterType.Name
             select new KeyValuePair<Type, MethodInfo>(parameterType, method))
            .ToDictionary(p => p.Key, p => p.Value);

        //static readonly MethodDictionary DeserializeMethods = GetDeserializeMethods(ref NullableGenericType);
        static readonly MethodDictionary DeserializeMethods = GetDeserializeMethods();

        public static readonly IEnumerable<Type> SupportedTypes = SerializeMethods.Keys;

        static SerializationStream()
        {
            foreach (var deserializeMethod in DeserializeMethods)
                if (!deserializeMethod.Value.IsGenericMethodDefinition)
                    _ = SerializationDelegateCache.GetFunc(deserializeMethod.Key);

            foreach (var serializeMethod in SerializeMethods)
                if (!serializeMethod.Value.IsGenericMethodDefinition)
                    _ = SerializationDelegateCache.GetSerializationMethod(serializeMethod.Key);

            //var ax = DeserializeMethods.Values.Where(p => p.IsGenericMethodDefinition);
            //var fx = DeserializeMethods.Values.Where(p => !p.IsGenericMethodDefinition);

            //var bx = SerializeMethods.Values.Where(p => p.IsGenericMethodDefinition);
            //var vx = SerializeMethods.Values.Where(p => !p.IsGenericMethodDefinition);

            //NonGenericSupportedTypes = SupportedTypes.Where(p => !p.GetTypeInfo().ContainsGenericParameters);
        }

        internal static ConcurrentCache<Type, TypeData> TypesCache = new ConcurrentCache<Type, TypeData>();
        internal static ConcurrentCache<Type, bool> KnownTypesCache = new ConcurrentCache<Type, bool>();



        public static bool IsKnownType(Type type)
        {
            if (KnownTypesCache.TryGetValue(type, out var result))
                return result;

            result =
                type.IsArray ||
                type == typeof(Uri) ||
                type == typeof(Guid) ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(TimeSpan) ||
                type == typeof(DateTime) ||
                type == typeof(SerializationStream) ||
                type == typeof(MemoryStream) ||
                type == typeof(BitSerializer) ||
                type.GetTypeInfo().IsEnum ||
                type.GetTypeInfo().IsPrimitive ||
                type == typeof(DateTimeOffset) ||
                type.IsInterfaceImplemented<IEnumerable>();

            _ = KnownTypesCache.TryAdd(type, result);

            return result;
        }

        static MethodDictionary GetSerializeMethods()
            => (from method in typeof(SerializationStream)
                    .GetMethodsInfo(name: nameof(SerializerOperation.Serialize), publicMethods: true, parametersCount: 1)
                let parameterType = method.GetParameters().Single().ParameterType
                where !parameterType.IsPointer
                orderby parameterType.Name
                select new KeyValuePair<Type, MethodInfo>(parameterType, method))
                .ToDictionary(p => p.Key, p => p.Value);

        //static MethodDictionary GetDeserializeMethods(ref Type nullableGenericType)
        static MethodDictionary GetDeserializeMethods()
        {
            var nonNullableOperationName = nameof(SerializerOperation.Deserialize);
            var nullableOperationName = $"{nonNullableOperationName}{nameof(Nullable)}";

            var deserializeMethods =
                (from method in typeof(SerializationStream)
                    .GetMethodsInfo(nameStartsWith: nameof(SerializerOperation.Deserialize), publicMethods: true, parametersCount: 0)
                 let returnType = method.ReturnType
                 let operationName = returnType.GetTypeInfo().IsValueType ? nonNullableOperationName : nullableOperationName
                 where method.Name.StartsWith(operationName, StringComparison.OrdinalIgnoreCase)
                    && !method.Name.Contains("raw", StringComparison.OrdinalIgnoreCase)
                    && !method.Name.EndsWith("enum", StringComparison.OrdinalIgnoreCase)
                    && !method.Name.Contains("collection", StringComparison.OrdinalIgnoreCase)
                    && !method.Name.Contains("dictionary", StringComparison.OrdinalIgnoreCase)
                    && !method.Name.Contains("compressed", StringComparison.OrdinalIgnoreCase)
                 orderby returnType.Name
                 select new KeyValuePair<Type, MethodInfo>(returnType, method))
                 .ToDictionary(p => p.Key, p => p.Value);

            if (typeof(SerializationStream).GetMethodInfo(nameof(SerializeEnum)) is MethodInfo serializeEnumMethodInfo)
                SerializeMethods.Add(typeof(Enum), serializeEnumMethodInfo);
            else
                throw new InvalidOperationException("");

            if (typeof(SerializationStream).GetMethodInfo(nameof(DeserializeEnum)) is MethodInfo deserializeEnumMethodInfo)
                deserializeMethods.Add(typeof(Enum), deserializeEnumMethodInfo);
            else
                throw new InvalidOperationException("");

            var notMappedTypes = SerializeMethods.Keys.Except(deserializeMethods.Keys);

            var sb = new StringBuilder();
            _ = sb.Append(Utilities.ResourceStrings.CyxorInternalException);

            var notMappedTypesCount = notMappedTypes.Count();

            if (notMappedTypesCount > 0)
            {
                var isAre = notMappedTypesCount == 1 ? "is" : "are";
                var typeTypes = notMappedTypesCount == 1 ? "type" : "types";

                _ = sb.Append($", there {isAre} '{notMappedTypesCount}' unmapped {typeTypes}:");

                var count = 0;

                foreach (var notMappedType in notMappedTypes)
                    _ = sb.Append($" {++count}- {notMappedType.Name}");
            }

            if (deserializeMethods.Count != SerializeMethods.Count)
                throw new InvalidOperationException(sb.ToString());

            //if (nullableGenericType == typeof(SerialStream))
            //    throw new InvalidOperationException(Utilities.ResourceStrings.CyxorInternalException);

            return deserializeMethods;
        }

        //        static MethodDictionary GetDeserializeMethods(ref Type nullableGenericType)
        //        {
        //            var methods = Utilities.Reflection.GetDeclaredPublicMethods(typeof(SerialStream));

        //            var deserializeMethods = new MethodDictionary();

        //            var nonNullableOperationName = nameof(SerializerOperation.Deserialize);
        //            var nullableOperationName = $"{nonNullableOperationName}{nameof(Nullable)}";

        //            foreach (var type in SerializeMethods.Keys)
        //            {
        //                //#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0
        //                //                if (type.Name.StartsWith("Span"))
        //                //                {
        //                //                    var isValueTypedd = type.GetTypeInfo().IsValueType || type == typeof(Enum);
        //                //                }
        //                //#endif


        //                var isValueType = type.GetTypeInfo().IsValueType || type == typeof(Enum);
        //                //#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0
        //                //                    || type == typeof(Span<>)
        //                //#endif
        //                //                    ;

        //                var operationName = isValueType ? nonNullableOperationName : nullableOperationName;

        //                var serializeTypeName = type.Name;

        //                if (serializeTypeName.StartsWith(nameof(Nullable), StringComparison.Ordinal))
        //                    serializeTypeName = nameof(Nullable) + Utilities.Reflection.GetGenericArguments(type)[0].Name;

        //                foreach (var method in methods)
        //                {
        //                    if (method.GetParameters().Length == 0 && method.Name.StartsWith(operationName, StringComparison.Ordinal))
        //                    {
        //                        //var typeName = method.Name.Substring(operationName.Length);

        //                        var genericArgumentsCount = method.GetGenericArguments().Length;

        //                        var typeName = method.Name.Substring(operationName.Length) +
        //                            (genericArgumentsCount == 0 ? string.Empty : $"`{genericArgumentsCount}");

        //                        //if (typeof(IGrouping<,>).Name.StartsWith(typeName, StringComparison.Ordinal))
        //                        //    typeName = typeof(IGrouping<,>).Name;

        //                        if (string.IsNullOrEmpty(typeName))
        //                            continue;

        //                        if (serializeTypeName != typeName)
        //#pragma warning disable IDE0057 // Substring can be simplified
        //                            if (serializeTypeName != typeName.Substring(0, typeName.Length - 1) + "[]")
        //#pragma warning restore IDE0057 // Substring can be simplified
        //                                if (!(serializeTypeName == nameof(IDictionary) && typeName == nameof(IEnumerable)))
        //                                    continue;
        //                                else if (method.GetGenericArguments().Length != 2)
        //                                    continue;

        //                        if (serializeTypeName == nameof(IEnumerable))
        //                            if (method.GetGenericArguments().Length != 1)
        //                                continue;

        //                        if (method.Name == nameof(DeserializeNullableT))
        //                            nullableGenericType = type;

        //                        deserializeMethods[type] = method;
        //                        break;
        //                    }
        //                }
        //            }

        //            var notMappedTypes = SerializeMethods.Keys.Except(deserializeMethods.Keys);

        //            var sb = new StringBuilder();
        //            _ = sb.Append(Utilities.ResourceStrings.CyxorInternalException);

        //            var notMappedTypesCount = notMappedTypes.Count();

        //            if (notMappedTypesCount > 0)
        //            {
        //                var isAre = notMappedTypesCount == 1 ? "is" : "are";
        //                var typeTypes = notMappedTypesCount == 1 ? "type" : "types";

        //                _ = sb.Append($", there {isAre} '{notMappedTypesCount}' unmapped {typeTypes}:");

        //                var count = 0;

        //                foreach (var notMappedType in notMappedTypes)
        //                    _ = sb.Append($" {++count}- {notMappedType.Name}");
        //            }

        //            if (deserializeMethods.Count != SerializeMethods.Count)
        //                throw new InvalidOperationException(sb.ToString());

        //            if (nullableGenericType == typeof(SerialStream))
        //                throw new InvalidOperationException(Utilities.ResourceStrings.CyxorInternalException);

        //            return deserializeMethods;
        //        }

        public static string GenerateSerializationSchema()
        {
            var result = new StringBuilder();

            foreach (var method in SerializeMethods)
            {
                _ = result.AppendLine(method.Value.ToString());

                _ = DeserializeMethods.ContainsKey(method.Key)
                    ? result.AppendLine($"'-->{DeserializeMethods[method.Key]}{Environment.NewLine}")
                    : result.AppendLine("Failed");
            }

            return result.ToString();
        }

#endregion Static

#region Reflector

        class ObjectSerialization
        {
            readonly bool Raw;

            internal int BufferPosition;
            internal int PositionLenght;

            internal ObjectSerialization? Next;
            internal ObjectSerialization? Previous;

            internal ObjectSerialization(int bufferPosition, bool raw, ObjectSerialization? previous)
            {
                Raw = raw;
                PositionLenght = 1;
                BufferPosition = bufferPosition;

                if (previous != default)
                {
                    Previous = previous;
                    previous.Next = this;
                }
            }

            void AdjustBufferPosition(int offset)
            {
                BufferPosition += offset;
                Next?.AdjustBufferPosition(offset);
            }

            internal void Step(SerializationStream serializer, int size)
            {
                var count = serializer.position + size - BufferPosition - PositionLenght;

                if (count < ObjectProperties.MaxLength || Raw)
                    return;

                var positionLenght =
                    count < Utilities.EncodedInteger.OneByteCap ? 2 :
                    count < Utilities.EncodedInteger.TwoBytesCap ? 3 :
                    count < Utilities.EncodedInteger.ThreeBytesCap ? 4 :
                    count < Utilities.EncodedInteger.FourBytesCap ? 5 : 6;

                if (PositionLenght < positionLenght)
                {
                    var offset = positionLenght - PositionLenght;

                    serializer.ObjectSerializationActive = !serializer.ObjectSerializationActive;
                    serializer.EnsureCapacity(offset + size, SerializerOperation.Serialize);
                    serializer.ObjectSerializationActive = !serializer.ObjectSerializationActive;

                    Buffer.BlockCopy(
                        serializer.buffer!,
                        BufferPosition,
                        serializer.buffer!,
                        BufferPosition + offset,
                        serializer.position - BufferPosition);

                    PositionLenght += offset;
                    serializer.position += offset;
                    Next?.AdjustBufferPosition(offset);
                }
            }
        }

        int CircularReferencesIndex;
        readonly bool CircularReferencesActive = true;
        readonly Dictionary<object, int> CircularReferencesDictionary = new Dictionary<object, int>();
        readonly Dictionary<int, object> CircularReferencesIndexDictionary = new Dictionary<int, object>();

        readonly bool UseObjectSerialization = true;
        bool ObjectSerializationActive;
        readonly Stack<ObjectSerialization> SerializationStack = new Stack<ObjectSerialization>();

        //static readonly MethodInfo DeserializeObjectMethodInfo = typeof(SerializationStream).GetMethodInfo(nameof(DeserializeObject), isPublic: true, parametersCount: 0, isGenericMethodDefinition: true, genericArgumentsCount: 1)!;

        internal static MethodInfo GetSerializationMethod(Type type, SerializerOperation operation)
        {
            var dictionary = operation == SerializerOperation.Serialize ? SerializeMethods : DeserializeMethods;

            if (type.IsByRef)
                type = type.GetElementType()!;

            var suitableType = type;
            var genericArgumentsType = default(Type[]);

            if (type.IsInterfaceImplemented<ISerializable>())
            {
                suitableType = typeof(ISerializable);
                genericArgumentsType = new Type[] { type };
            }
            else if (type.GetTypeInfo().IsEnum)
            {
                suitableType = typeof(Enum);
                genericArgumentsType = new Type[] { type };
            }
            else if (type.IsArray && type.GetElementType() != typeof(byte) && type.GetElementType() != typeof(char))
            {
                suitableType = typeof(Array);
                genericArgumentsType = new Type[] { type.GetElementType()! };
            }
            else if (type != typeof(string) && type.IsInterfaceImplemented<IEnumerable>())
            {
                if (type.GetTypeInfo().IsGenericType)
                {
                    genericArgumentsType = type.GetGenericArguments();

                    if (genericArgumentsType.Length == 1)
                        suitableType = typeof(IEnumerable);
                    else if (genericArgumentsType.Length == 2)
                        suitableType = type.Name == typeof(IGrouping<,>).Name
                            ? typeof(IGrouping<,>)
                            : typeof(IDictionary);
                }
            }

            if (!dictionary.TryGetValue(suitableType, out var method))
            {
                //if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                //{
                //    if (!dictionary.TryGetValue(NullableGenericType, out method))
                //        throw new InvalidOperationException(Utilities.ResourceStrings.CyxorInternalException);
                //}
                //else
                if (type.GetTypeInfo().IsValueType)
                {
                    //method = DeserializeObjectMethodInfo;
                    method = typeof(SerializationStream).GetMethodInfo(nameof(DeserializeObject), isPublic: true, parametersCount: 0, isGenericMethodDefinition: true, genericArgumentsCount: 1)!;
                    genericArgumentsType = new Type[] { type };
                }
                else
                {
                    method = dictionary[typeof(object)];
                    genericArgumentsType = new Type[] { type };
                }
            }

            if (method.IsGenericMethodDefinition)
                method = method.MakeGenericMethod(genericArgumentsType ?? type.GetGenericArguments());

            return method;
        }

#endregion Reflector

#region Core

        public SerializationStream() { }

        public SerializationStream(byte[] value)
            => SetBuffer(value);

        public SerializationStream(string value)
            => SerializeRaw(value);

        public SerializationStream(object value)
            => SerializeRaw(value);

        public SerializationStream(SerializationStream value)
            => SerializeRaw(value);

        public SerializationStream(ArraySegment<byte> arraySegment)
            => SetBuffer(arraySegment);

        public SerializationStream(byte[] buffer, int offset, int count)
            => SetBuffer(buffer, offset, count);

        public SerializationStream(object value, IBackingSerializer backingSerializer, object? backingSerializerOptions = default)
            => SerializeRaw(value, backingSerializer, backingSerializerOptions);

#region Public properties

        /// <summary>
        /// Represents the empty Serializer. This field is readonly.
        /// </summary>
        public static readonly SerializationStream Empty = new SerializationStream();

        static readonly UTF8Encoding Encoding = new UTF8Encoding(false, true);

        static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

//#if !NETSTANDARD1_0
//        public static ASCIIEncoding ASCIIEncoding = new ASCIIEncoding();
//#endif


        /// <summary>
        /// Indicates the Serializer default byte order ("endianess").
        /// All values written to serializer instances will be converted to the specified byte order if necessary and
        /// values read will be converted to this computer architecture byte order if necessary. The initial value
        /// is always equal to the current computer architecture and by default swapping never occurs.
        /// </summary>
        /// <value>
        /// The byte order.
        /// </value>
        //public static ByteOrder ByteOrder;

        /// <summary>
        /// The maximum number of bytes this buffer can hold.
        /// </summary>
        /// <remarks>
        /// This value is never bigger than the maximum value of a signed 32-bit integer.
        /// </remarks>
        /// <returns>
        /// The maximum number of bytes this buffer can hold.
        /// </returns>
        public const int MaxCapacity = int.MaxValue - 64;

        readonly bool readOnly;

        int position;
        /// <summary>
        /// Get and set the current position within the buffer.
        /// </summary>
        /// <returns>
        /// The current position within the buffer.
        /// </returns>
        public int Int32Position
        {
            get => position;
            set
            {
                if (value < 0 || value > length)
                    throw new ArgumentOutOfRangeException(nameof(value));

                position = value;
            }
        }

        public int Count => length - position;

#pragma warning disable IDE0032 // Use auto property
        int length;
#pragma warning restore IDE0032 // Use auto property

        /// <summary>
        /// Gets and sets the length of the buffer to the specified value.
        /// </summary>
        public int Int32Length => length;

        /// <remarks>
        /// If the value is bigger than the actual Length the buffer will try to expand. The Position within
        /// the buffer remains unchangeable, but the <see cref="Capacity"/> increments if necessary.
        ///
        /// If the value is smaller than the actual Length the Length is updated with the new value. If the
        /// original Position is smaller than the new Length value it remains unchangeable, otherwise it is set
        /// to the value of Length.
        /// </remarks>
        /// <returns>
        /// The length of the buffer in bytes.
        /// </returns>
        /// <param name="value">
        /// The value at which to set the length.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value"/> is negative or is greater than the maximum capacity of the Buffer,
        /// where the maximum length is <see cref="MaxCapacity"/>.
        /// </exception>
        public void SetLength(int value)
        {
            if (value < 0 || value > MaxCapacity)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (length == value)
                return;

            var startPosition = position;

            if (value > Capacity)
            {
                position = 0;
                EnsureCapacity(value, SerializerOperation.Serialize);
            }
            else
                length = value;

            position = startPosition < length ? startPosition : length;
        }

        /// <summary>
        /// Gets the number of bytes allocated for this serializer.
        /// </summary>
        /// <returns>
        /// The length of the usable portion of the serializer buffer.
        /// </returns>
        int Capacity;

        public int GetCapacity()
            => Capacity;

        byte[]? buffer;

        /// <summary>
        /// Gets the underlying array of unsigned bytes of this SerialStream.
        /// </summary>
        /// <returns>
        /// The underlying array of unsigned bytes of this SerialStream.
        /// </returns>
        public byte[]? GetBuffer()
            => buffer;

#endregion

        public event EventHandler? MemoryDisposed;

#region Public

        public static bool IsNullOrEmpty(SerializationStream serializer)
            => (serializer?.length ?? 0) == 0 ? true : false;

        public string ToHexString()
        {
            var hex = buffer != default ? BitConverter.ToString(buffer, 0, length) : "null";
            return $"[{position}-{length}-{Capacity}]  {hex}";
        }

        string DebuggerDisplay()
        {
            var count = length > 20 ? 20 : length;

            var sb = new StringBuilder(count * 2 + 3);

            if (length == 0)
                _ = sb.Append(buffer == default ? "null" : $"byte[{buffer.Length}]");
            else
                for (var i = 0; i < count; _ = sb.Append('-'))
                    _ = sb.Append(buffer![i++]);

            if (length > count)
                _ = sb.Insert(sb.Length - 1, "...");

            if (length != 0)
                _ = sb.Remove(sb.Length - 1, 1);

            return $"[{nameof(position)} = {position}]  [{nameof(length)} = {length}]  [{nameof(Capacity)} = {Capacity}]  [Data = {sb}]";
        }

        public void SetBuffer(byte[] buffer)
            => SetBuffer(buffer, 0, buffer.Length);

        public void SetBuffer(ArraySegment<byte> arraySegment)
            => SetBuffer(arraySegment.Array, arraySegment.Offset, arraySegment.Count);

        public void SetBuffer(byte[]? value, int index, int count)
        {
            if (count < 0 || count > (value?.Length ?? 0))
                throw new ArgumentOutOfRangeException(nameof(count));

            if (index < 0 || index > count)
                throw new ArgumentOutOfRangeException(nameof(index));

            InternalSetBuffer(value);

            length = count;
            position = index;
            Capacity = buffer?.Length ?? 0;
        }

        void InternalSetBuffer(byte[]? value)
        {
            if (buffer != default)
#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0
            {
                BufferPool.Return(buffer);
#endif
                MemoryDisposed?.Invoke(this, EventArgs.Empty);
#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0
            }
#endif

            buffer = value;
        }

        static byte[] CreateBuffer(int length)
#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0
            => BufferPool.Rent(length);
#else
            => new byte[length];
#endif

        /// <summary>
        /// Sets the length of the current buffer to the specified value.
        /// </summary>
        /// <remarks>
        /// This method behaves similar to SetLength() except that it doesn't modified the current <see cref="length"/>
        /// if the <paramref name="value"/> is not smaller than current <see cref="Capacity"/>.
        /// </remarks>
        /// <param name="value">
        /// The value at which to set the length.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="value"/> is negative or is greater than the maximum capacity of the Buffer,
        /// where the maximum capacity is <see cref="MaxCapacity"/>.
        /// </exception>
        public void SetCapacity(int value)
        {
            if (value < 0 || value > MaxCapacity)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (Capacity == value)
                return;

            var startLength = length;
            var startPosition = position;

            if (value > Capacity)
            {
                position = 0;
                EnsureCapacity(value, SerializerOperation.Serialize);
                length = startLength;
            }
            else
            {
                InternalSetBuffer(default);

                Capacity = length = position = 0;
                EnsureCapacity(value, SerializerOperation.Serialize);

                if (length > startLength)
                    length = startLength;
            }

            position = startPosition < length ? startPosition : length;
        }

        /// <summary>
        /// Quick reset of the buffer content by setting the <see cref="length"/> and <see cref="Int32Position"/> to 0.
        /// The actual allocated <see cref="Capacity"/> remains intact.
        /// </summary>
        public void Reset()
            => Reset(0);

        /// <summary>
        /// Resets the contents of the buffer.
        /// </summary>
        /// <remarks>
        /// The parameter <paramref name="numBytes"/> suggest when to perform a full reset or not. If the actual
        /// <see cref="Capacity"/> is greater than <paramref name="numBytes"/> the buffer is fully reseted and
        /// <see cref="Capacity"/> is set to zero. If the <see cref="Capacity"/> is lower than <paramref name="numBytes"/>
        /// or <paramref name="numBytes"/> is equal to zero this method behaves the same as Reset().
        /// For setting the buffer <see cref="Capacity"/> to an exact value use SetCapacity().
        /// </remarks>
        /// <param name="numBytes">
        /// The maximum number of bytes allowed to remain allocated without performing a full reset.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="numBytes"/> is negative or is greater than the maximum capacity of the Buffer,
        /// where the maximum capacity is <see cref="MaxCapacity"/>.
        /// </exception>
        public void Reset(int numBytes)
        {
            if (numBytes < 0 || numBytes > MaxCapacity)
                throw new ArgumentOutOfRangeException(nameof(numBytes));

            if (Capacity <= numBytes || numBytes == 0)
                length = position = 0;
            else
            {
                Capacity = length = position = 0;
                InternalSetBuffer(default);
            }
        }

        public void Insert(SerializationStream value)
        {
            if (value.length == 0)
                return;

            EnsureCapacity(value.length, SerializerOperation.Serialize);

            using var auxBuffer = new SerializationStream();
            auxBuffer.SerializeRaw(buffer, position, length - position);
            SerializeRaw(value);
            SerializeRaw(auxBuffer);
        }

        public void Delete()
            => Delete(0, 0);

        public void Delete(int index)
            => Delete(index, 0);

        // TODO: Check this method
        public void Delete(int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), Utilities.ResourceStrings.ExceptionNegativeNumber);

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), Utilities.ResourceStrings.ExceptionNegativeNumber);

            if (length - index < count)
                throw new ArgumentException($"{nameof(length)} - {nameof(index)} < {nameof(count)}");

            //if (index > Length)
            //    throw new IndexOutOfRangeException();

            //if (count == 0 && index != 0)
            //    throw new ArgumentException();

            //if ((uint)(index + count) > Length)
            //    throw new IndexOutOfRangeException();

            if (index == 0 && count == 0)
            {
                Capacity = length = position = 0;
                InternalSetBuffer(default);
            }
            else if (index == 0 && count == length)
            {
                length = position = 0;
            }
            else
            {
                System.Buffer.BlockCopy(buffer!, index + count, buffer!, index, length - count);

                position = index;
                length -= count;
            }
        }

        public void Pop(int count)
            => Delete(0, count);

        public void PopChars(int count)
        {
            if (count < 0 || count > length)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0)
                return;

            position = 0;

            for (var i = 0; i < count; i++)
                _ = DeserializeChar();

            count = position;

            System.Buffer.BlockCopy(buffer!, count, buffer!, 0, length -= count);

            position = 0;
        }

        /// <summary>
        /// Returns the hash code for this buffer.
        /// </summary>
        /// <remarks>
        /// This method uses all the bytes in the buffer to generate a reasonably randomly distributed output
        /// that is suitable for use in hashing algorithms and data structures such as a hash table.
        /// </remarks>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode()
            => Utilities.HashCode.GetFrom(buffer!, 0, length);

        public object Clone()
        {
            var value = new SerializationStream();
            value.SerializeRaw(this);

            value.length = length;
            value.position = position;

            return value;
        }

#endregion Public

#region Private

        public void EnsureCapacity(int size)
            => EnsureCapacity(size, SerializerOperation.Serialize);

        void EnsureCapacity(int size, SerializerOperation operation)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            if (operation != SerializerOperation.Deserialize && readOnly)
                throw new InvalidOperationException("The buffer is marked as ReadOnly.");

            if ((uint)(position + size) > length)
            {
                if (operation == SerializerOperation.Deserialize)
                    throw new BufferOverflowException();
                else
                {
                    if (position + size < 0)
                        throw new BufferOverflowException();
                    else if (position + size <= Capacity)
                        length = position + size;
                    else
                    {
                        if (buffer == default)
                        {
                            Capacity = length = size;

                            if (Capacity < 256)
                                Capacity = 256;

                            buffer = CreateBuffer(Capacity);
                        }
                        else if (position + size > Capacity)
                        {
                            if (position + size > (uint)(Capacity + Capacity / 2))
                                Capacity = position + size;
                            else if ((uint)(Capacity + Capacity / 2) >= MaxCapacity)
                                Capacity = MaxCapacity;
                            else
                                Capacity += Capacity / 2;

                            var newBuffer = CreateBuffer(Capacity);

                            System.Buffer.BlockCopy(buffer, 0, newBuffer, 0, length);

                            length = position + size;
                            InternalSetBuffer(newBuffer);
                        }
                    }
                }
            }

            if (UseObjectSerialization)
                if (ObjectSerializationActive && operation == SerializerOperation.Serialize)
                {
                    ObjectSerialization? objectSerialization;
                    objectSerialization = SerializationStack.Peek();

                    while (objectSerialization != default)
                    {
                        objectSerialization.Step(this, size);
                        objectSerialization = objectSerialization.Previous;
                    }
                }
        }

#endregion Private

#endregion Core

#region Helper
        void SerializeOp(int count)
        {
            if (count < ObjectProperties.MaxLength)
                Serialize((byte)(count << 2));
            else
            {
                Serialize(ObjectProperties.LengthMap);
                Serialize(count);
            }
        }

        int DeserializeOp()
        {
            var op = DeserializeByte();

            return op == 0 ? -1
                : op == ObjectProperties.EmptyMap ? 0
                : op == ObjectProperties.LengthMap ? DeserializeInt32()
                : ObjectProperties.Length(op);
        }

#endregion
    }
}