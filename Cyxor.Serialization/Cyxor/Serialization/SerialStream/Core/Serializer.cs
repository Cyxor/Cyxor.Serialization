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
    using Extensions;

    using MethodDictionary = Dictionary<Type, MethodInfo>;
    using BufferOverflowException = EndOfStreamException;

    [DebuggerDisplay("{DebuggerDisplay()}")]
    public sealed partial class Serializer : Stream
    {
        bool AutoRaw;

#region Reflector

        sealed class ObjectSerialization
        {
            readonly bool Raw;

            public int BufferPosition;
            public int PositionLenght;

            public ObjectSerialization? Next;
            public ObjectSerialization? Previous;

            public ObjectSerialization(int bufferPosition, bool raw, ObjectSerialization? previous)
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Update(Serializer serializer, int size)
            {
                var count = serializer._position + size - BufferPosition - PositionLenght;

                if (count < ObjectLengthMap || Raw)
                    return;

                var positionLenght =
                    count < Utilities.EncodedInteger.OneByteCap ? 2 :
                    count < Utilities.EncodedInteger.TwoBytesCap ? 3 :
                    count < Utilities.EncodedInteger.ThreeBytesCap ? 4 :
                    count < Utilities.EncodedInteger.FourBytesCap ? 5 : 6;

                if (PositionLenght < positionLenght)
                {
                    var offset = positionLenght - PositionLenght;

                    serializer.PrefixObjectLengthActive = false;
                    serializer.EnsureCapacity(offset + size, SerializerOperation.Serialize);
                    serializer.PrefixObjectLengthActive = true;

                    Buffer.BlockCopy(
                        serializer._buffer!,
                        BufferPosition,
                        serializer._buffer!,
                        BufferPosition + offset,
                        serializer._position - BufferPosition);

                    PositionLenght += offset;
                    serializer._position += offset;

                    for (var next = Next; next != null; next = next.Next)
                        next.BufferPosition += offset;
                }
            }
        }

        int CircularReferencesIndex;
        readonly Dictionary<object, int> CircularReferencesSerializeDictionary = new Dictionary<object, int>();
        readonly Dictionary<int, object> CircularReferencesDeserializeDictionary = new Dictionary<int, object>();

        bool PrefixObjectLengthActive;
        readonly Stack<ObjectSerialization> SerializationStack = new Stack<ObjectSerialization>();

        //static readonly MethodInfo DeserializeObjectMethodInfo = typeof(SerializationStream).GetMethodInfo(nameof(DeserializeObject), isPublic: true, parametersCount: 0, isGenericMethodDefinition: true, genericArgumentsCount: 1)!;

        #endregion Reflector

        #region Core

        readonly SerializerOptions Options;

#region Public properties

        /// <summary>
        /// Represents the empty Serializer. This field is readonly.
        /// </summary>
        public static readonly Serializer Empty = new Serializer();

        //static readonly UTF8Encoding Encoding = new UTF8Encoding(false, true);

        static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        /// <summary>
        /// The maximum number of bytes this buffer can hold. This value is equal to <see cref="
        /// int.MaxValue"/>.
        /// </summary>
        /// <returns>
        /// The maximum number of bytes this buffer can hold.
        /// </returns>
        public const int MaxCapacity = int.MaxValue;

        readonly bool readOnly;

        int _position;
        /// <summary>
        /// Get and set the current position within the buffer.
        /// </summary>
        /// <returns>
        /// The current position within the buffer.
        /// </returns>
        public int Int32Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > _length)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _position = value;
            }
        }

        public int Count => _length - _position;

#pragma warning disable IDE0032 // Use auto property
        int _length;
#pragma warning restore IDE0032 // Use auto property

        /// <summary>
        /// Gets and sets the length of the buffer to the specified value.
        /// </summary>
        public int Int32Length => _length;

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

            if (_length == value)
                return;

            var startPosition = _position;

            if (value > Capacity)
            {
                _position = 0;
                EnsureCapacity(value, SerializerOperation.Serialize);
            }
            else
                _length = value;

            _position = startPosition < _length ? startPosition : _length;
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

        Stream? _stream;

        Memory<byte> _memory;

        byte[]? _buffer;

        /// <summary>
        /// Gets the underlying array of unsigned bytes of this SerialStream.
        /// </summary>
        /// <returns>
        /// The underlying array of unsigned bytes of this SerialStream.
        /// </returns>
        public byte[]? GetBuffer()
            => _buffer;

#endregion

        //public event EventHandler? MemoryDisposed;

#region Public

        public static bool IsNullOrEmpty(Serializer serializer)
            => (serializer?._length ?? 0) == 0 ? true : false;

        public string ToHexString()
        {
            var hex = _buffer != default ? BitConverter.ToString(_buffer, 0, _length) : "null";
            return $"[{_position}-{_length}-{Capacity}]  {hex}";
        }

        string DebuggerDisplay()
        {
            var count = _length > 20 ? 20 : _length;

            var sb = new StringBuilder(count * 2 + 3);

            if (_length == 0)
                _ = sb.Append(_buffer == default ? "null" : $"byte[{_buffer.Length}]");
            else
                for (var i = 0; i < count; _ = sb.Append('-'))
                    _ = sb.Append(_buffer![i++]);

            if (_length > count)
                _ = sb.Insert(sb.Length - 1, "...");

            if (_length != 0)
                _ = sb.Remove(sb.Length - 1, 1);

            return $"[{nameof(_position)} = {_position}]  [{nameof(_length)} = {_length}]  [{nameof(Capacity)} = {Capacity}]  [Data = {sb}]";
        }

        public void SetBuffer(byte[] buffer)
            => SetBuffer(buffer, 0, buffer.Length);

        public void SetBuffer(ArraySegment<byte> arraySegment)
            => SetBuffer(arraySegment.Array, arraySegment.Offset, arraySegment.Count);

        public void SetBuffer(byte[]? value, int start, int length)
        {
            if (length < 0 || length > (value?.Length ?? 0))
                throw new ArgumentOutOfRangeException(nameof(length));

            if (start < 0 || start > (value?.Length ?? 0))
                throw new ArgumentOutOfRangeException(nameof(start));

            //var memoryOwner = MemoryPool<byte>.Shared.Rent();
            //if (_memory != null)

            InternalSetBuffer(value);

            _length = length;
            _position = start;
            Capacity = _buffer?.Length ?? 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalSetBuffer(byte[]? value)
        {
            if (_buffer != null)
            {
                if (Options.Pooling && _buffer.Length >= Options.PoolThreshold)
                    BufferPool.Return(_buffer);

                //MemoryDisposed?.Invoke(this, EventArgs.Empty);
            }

            _buffer = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte[] CreateBuffer(int length)
            => Options.Pooling && length >= Options.PoolThreshold
                ? BufferPool.Rent(length)
                : new byte[length];

        /// <summary>
        /// Sets the length of the current buffer to the specified value.
        /// </summary>
        /// <remarks>
        /// This method behaves similar to SetLength() except that it doesn't modified the current <see cref="_length"/>
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

            var startLength = _length;
            var startPosition = _position;

            if (value > Capacity)
            {
                _position = 0;
                EnsureCapacity(value, SerializerOperation.Serialize);
                _length = startLength;
            }
            else
            {
                InternalSetBuffer(default);

                Capacity = _length = _position = 0;
                EnsureCapacity(value, SerializerOperation.Serialize);

                if (_length > startLength)
                    _length = startLength;
            }

            _position = startPosition < _length ? startPosition : _length;
        }

        /// <summary>
        /// Quick reset of the buffer content by setting the <see cref="_length"/> and <see cref="Int32Position"/> to 0.
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
                _length = _position = 0;
            else
            {
                Capacity = _length = _position = 0;
                InternalSetBuffer(default);
            }
        }

        public void Insert(Serializer value)
        {
            if (value._length == 0)
                return;

            EnsureCapacity(value._length, SerializerOperation.Serialize);

            using var auxBuffer = new Serializer();
            auxBuffer.SerializeRaw(_buffer, _position, _length - _position);
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

            if (_length - index < count)
                throw new ArgumentException($"{nameof(_length)} - {nameof(index)} < {nameof(count)}");

            //if (index > Length)
            //    throw new IndexOutOfRangeException();

            //if (count == 0 && index != 0)
            //    throw new ArgumentException();

            //if ((uint)(index + count) > Length)
            //    throw new IndexOutOfRangeException();

            if (index == 0 && count == 0)
            {
                Capacity = _length = _position = 0;
                InternalSetBuffer(default);
            }
            else if (index == 0 && count == _length)
            {
                _length = _position = 0;
            }
            else
            {
                System.Buffer.BlockCopy(_buffer!, index + count, _buffer!, index, _length - count);

                _position = index;
                _length -= count;
            }
        }

        public void Pop(int count)
            => Delete(0, count);

        public void PopChars(int count)
        {
            if (count < 0 || count > _length)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0)
                return;

            _position = 0;

            for (var i = 0; i < count; i++)
                _ = DeserializeChar();

            count = _position;

            System.Buffer.BlockCopy(_buffer!, count, _buffer!, 0, _length -= count);

            _position = 0;
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
            => Utilities.HashCode.GetFrom(_buffer!, 0, _length);

        public object Clone()
        {
            var value = new Serializer();
            value.SerializeRaw(this);

            value._length = _length;
            value._position = _position;

            return value;
        }

        #endregion Public

        #region Private

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void EnsureCapacity(int size, SerializerOperation operation)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            if (operation == SerializerOperation.Serialize && readOnly)
                throw new InvalidOperationException("The buffer is marked as ReadOnly.");

            var newPosition = _position + size;

            if (newPosition < 0 || newPosition > MaxCapacity)
                throw new BufferOverflowException();

            if (newPosition > _length)
            {
                if (operation == SerializerOperation.Deserialize)
                    throw new BufferOverflowException();
                else
                {
                    if (newPosition <= Capacity)
                        _length = newPosition;
                    else
                    {
                        if (_buffer == null)
                        {
                            _length = size;
                            Capacity = size > 256 ? size : 256;
                            _buffer = CreateBuffer(Capacity);
                            //buffer = Options.Pooling && Capacity >= Options.PoolThreshold
                            //    ? BufferPool.Rent(Capacity)
                            //    : new byte[Capacity];
                        }
                        else if (newPosition > Capacity)
                        {
                            var newCapacity = Capacity + Capacity;
                            newCapacity = newCapacity < 0 || newCapacity > MaxCapacity ? MaxCapacity : newCapacity;
                            Capacity = newPosition > newCapacity ? newPosition : newCapacity;

                            var newBuffer = CreateBuffer(Capacity);
                            //newBuffer = Options.Pooling && Capacity >= Options.PoolThreshold
                            //    ? BufferPool.Rent(Capacity)
                            //    : new byte[Capacity];

                            _buffer.AsSpan(0.._length).CopyTo(newBuffer.AsSpan(0.._length));

                            _length = newPosition;
                            InternalSetBuffer(newBuffer);
                        }
                    }
                }
            }

            if (PrefixObjectLengthActive && operation == SerializerOperation.Serialize)
            {
                ObjectSerialization? obj;

                for (obj = SerializationStack.Peek(); obj != null; obj = obj.Previous)
                    obj.Update(this, size);
            }
        }

        #endregion Private

        #endregion Core

        #region Helper

        const byte ObjectLengthMap = 0b_00_111111; // 63
        const byte SequenceLengthMap = 0b_0_1111111; // 127

        const byte EmptyMap = 0b_1_0000000; // 128

        const byte PartialMap = 0b_01_000000; // 64
        const byte CircularMap = 0b_11_000000; // 192

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SerializeSequenceHeader(int count)
        {
            if (count < SequenceLengthMap)
                Serialize((byte)count);
            else
                Serialize(count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int DeserializeSequenceHeader()
        {
            var op = DeserializeByte();

            if (op == 0)
                return -1;
            else if (op == EmptyMap)
                return 0;
            else if ((op & EmptyMap) != EmptyMap)
                return op & SequenceLengthMap;
            else
            {
                var val1 = (uint)op & 127;
                var val2 = 7;

                while (val2 != 35)
                {
                    op = DeserializeByte();
                    val1 |= ((uint)op & 127) << val2;
                    val2 += 7;

                    if ((op & 128) == 0)
                        return (int)(val1 >> 1) ^ -(int)(val1 & 1);
                }

                throw new InvalidOperationException("Invalid deserialize sequence header");
            }
        }

        #endregion
    }
}