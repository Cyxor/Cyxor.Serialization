using System;
using System.IO;
using System.Text;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization
{
    [DebuggerDisplay("{DebuggerDisplay()}")]
    public sealed partial class Serializer : Stream //, IBufferWriter<byte>
    {
        bool AutoRaw;

        #region Reflector

        sealed class ObjectSerialization
        {
            readonly bool Raw;

            public int BufferPosition;
            public int PositionLength;

            public ObjectSerialization? Next;
            public ObjectSerialization? Previous;

            public ObjectSerialization(int bufferPosition, bool raw, ObjectSerialization? previous)
            {
                Raw = raw;
                PositionLength = 1;
                BufferPosition = bufferPosition;

                if (previous != default)
                {
                    Previous = previous;
                    previous.Next = this;
                }
            }

            public void Update(Serializer serializer, int size)
            {
                var count = serializer._position + size - BufferPosition - PositionLength;

                if (count < ObjectLengthMap || Raw)
                    return;

                var positionLenght = count < Utilities.EncodedInteger.OneByteCap
                    ? 2
                    : count < Utilities.EncodedInteger.TwoBytesCap
                            ? 3
                            : count < Utilities.EncodedInteger.ThreeBytesCap
                                    ? 4
                                    : count < Utilities.EncodedInteger.FourBytesCap ? 5 : 6;

                if (PositionLength < positionLenght)
                {
                    var offset = positionLenght - PositionLength;

                    serializer.PrefixObjectLengthActive = false;
                    serializer.InternalEnsureSerializeCapacity(offset + size);
                    serializer.PrefixObjectLengthActive = true;

                    Buffer.BlockCopy(
                        serializer._buffer!,
                        BufferPosition,
                        serializer._buffer!,
                        BufferPosition + offset,
                        serializer._position - BufferPosition
                    );

                    PositionLength += offset;
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

#pragma warning disable IDE0032 // Use auto property
        int _length;
#pragma warning restore IDE0032 // Use auto property
        int _position;
        int _capacity;
        bool _disposed;
        Memory<byte> _memory;
        readonly Stream? _stream;
        readonly bool _needDisposeBuffer;
        IMemoryOwner<byte>? _memoryOwner;
        readonly SerializerOptions _options;

        public Span<byte> Span => AsSpan<byte>();

        //byte[]? _buffer;


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

        /// <summary>
        /// Gets and sets the length of the buffer to the specified value.
        /// </summary>
        public int Int32Length => _length;

        /// <remarks>
        /// If the value is bigger than the actual Length the buffer will try to expand. The Position within
        /// the buffer remains unchangeable, but the <see cref="_capacity"/> increments if necessary.
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

            if (value > _capacity)
            {
                _position = 0;
                InternalEnsureSerializeCapacity(value);
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
        public int GetCapacity() => _capacity;

        /// <summary>
        /// Gets the underlying array of unsigned bytes of this SerialStream.
        /// </summary>
        /// <returns>
        /// The underlying array of unsigned bytes of this SerialStream.
        /// </returns>
        public byte[]? GetBuffer() => _buffer;

        #endregion

        //public event EventHandler? MemoryDisposed;

        #region Public

        public static bool IsNullOrEmpty(Serializer serializer) => (serializer?._length ?? 0) == 0 ? true : false;

        public string ToHexString()
        {
            var hex = _buffer != default ? BitConverter.ToString(_buffer, 0, _length) : "null";
            return $"[{_position}-{_length}-{_capacity}]  {hex}";
        }

        string DebuggerDisplay()
        {
            var count = _length > 20 ? 20 : _length;

            var sb = new StringBuilder(count * 2 + 3);

            if (_length == 0)
                _ = sb.Append(_memory.IsEmpty == default ? "null" : $"byte[{_memory.Length}]");
            else
                for (var i = 0; i < count; _ = sb.Append('-'))
                    _ = sb.Append(_memory.Span[i++]);

            if (_length > count)
                _ = sb.Insert(sb.Length - 1, "...");

            if (_length != 0)
                _ = sb.Remove(sb.Length - 1, 1);

            return $"[{nameof(_position)} = {_position}]  [{nameof(_length)} = {_length}]  [{nameof(_capacity)} = {_capacity}]  [Data = {sb}]";
        }

        public void SetBuffer(byte[]? buffer) => SetBuffer(buffer, 0, buffer?.Length ?? 0);

        public void SetBuffer(ArraySegment<byte> arraySegment) =>
            SetBuffer(arraySegment.Array, arraySegment.Offset, arraySegment.Count);

        public void SetBuffer(byte[]? value, int start, int length)
        {
            if (length < 0 || length > (value?.Length ?? 0))
                throw new ArgumentOutOfRangeException(nameof(length));

            if (start < 0 || start > (value?.Length ?? 0))
                throw new ArgumentOutOfRangeException(nameof(start));

            //var memoryOwner = MemoryPool<byte>.Shared.Rent();
            if (_memoryOwner != null)
            {
                _memoryOwner.Dispose();
                _memoryOwner = null;
            }

            _memory = new Memory<byte>(value, start, length);

            //_memory.

            InternalSetBuffer(value);

            _length = length;
            _position = start;
            _capacity = _buffer?.Length ?? 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalSetBuffer(byte[]? value)
        {
            if (_buffer != null)
            {
                if (_options.Pooling && _buffer.Length >= _options.PoolThreshold)
                    BufferPool.Return(_buffer);
                //MemoryDisposed?.Invoke(this, EventArgs.Empty);
            }

            _buffer = value;
        }

        /// <summary>
        /// Sets the length of the current buffer to the specified value.
        /// </summary>
        /// <remarks>
        /// This method behaves similar to SetLength() except that it doesn't modified the current <see cref="_length"/>
        /// if the <paramref name="value"/> is not smaller than current <see cref="_capacity"/>.
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

            if (_capacity == value)
                return;

            var startLength = _length;
            var startPosition = _position;

            if (value > _capacity)
            {
                _position = 0;
                InternalEnsureSerializeCapacity(value);
                _length = startLength;
            }
            else
            {
                InternalSetBuffer(default);

                _capacity = _length = _position = 0;
                InternalEnsureSerializeCapacity(value);

                if (_length > startLength)
                    _length = startLength;
            }

            _position = startPosition < _length ? startPosition : _length;
        }

        /// <summary>
        /// Quick reset of the buffer content by setting the <see cref="_length"/> and <see cref="Int32Position"/> to 0.
        /// The actual allocated <see cref="_capacity"/> remains intact.
        /// </summary>
        public void Reset() => Reset(0);

        /// <summary>
        /// Resets the contents of the buffer.
        /// </summary>
        /// <remarks>
        /// The parameter <paramref name="numBytes"/> suggest when to perform a full reset or not. If the actual
        /// <see cref="_capacity"/> is greater than <paramref name="numBytes"/> the buffer is fully reseted and
        /// <see cref="_capacity"/> is set to zero. If the <see cref="_capacity"/> is lower than <paramref name="numBytes"/>
        /// or <paramref name="numBytes"/> is equal to zero this method behaves the same as Reset().
        /// For setting the buffer <see cref="_capacity"/> to an exact value use SetCapacity().
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

            if (_capacity <= numBytes || numBytes == 0)
                _length = _position = 0;
            else
            {
                _capacity = _length = _position = 0;
                InternalSetBuffer(default);
            }
        }

        public void Insert(Serializer value)
        {
            if (value._length == 0)
                return;

            InternalEnsureSerializeCapacity(value._length);

            using var auxBuffer = new Serializer();
            auxBuffer.SerializeRaw(_buffer, _position, _length - _position);
            SerializeRaw(value);
            SerializeRaw(auxBuffer);
        }

        public void Delete() => Delete(0, 0);

        public void Delete(int index) => Delete(index, 0);

        //start is less than zero or greater than System.Memory`1.Length. /// -or-
        //     /// length is greater than System.Memory`1.Length - start
        public void Delete(int start, int length)
        {
            if (start < 0 || start > _length)
                throw new ArgumentOutOfRangeException(
                    nameof(start),
                    "start is less than zero or greater than the serializer length"
                );

            if (length > _length - start)
                throw new ArgumentOutOfRangeException(
                    nameof(length),
                    "length is greater than the serializer length - start"
                );

            if (_stream != null)
                return;

            if (start + length == _length) { }
            else { }

            if (index == 0 && count == 0)
            {
                _capacity = _length = _position = 0;
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

        public void Pop(int count) => Delete(0, count);

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

        public void Slice(int start)
        {
            if (_stream != null)
                throw new InvalidOperationException("This serializer instance do not support slicing");

            _memory = _memory.Slice(start);
            _position -= _position <= start ? _position : start;
            _length -= start;
            _capacity -= start;
        }

        public void Slice(int start, int length)
        {
            if (_stream != null)
                throw new InvalidOperationException("This serializer instance do not support slicing");

            _memory = _memory.Slice(start, length);
            _position = _position <= start ? 0 : _position > length ? length : start;
            _length -= start;
            _capacity = length;
        }

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
        void InternalExpandMemory(int capacity)
        {
            var newCapacity = _capacity + _capacity;
            newCapacity = newCapacity < 0 || newCapacity > MaxCapacity
                ? MaxCapacity
                : newCapacity > capacity ? newCapacity : capacity < 256 ? 256 : capacity;

            if (_options.Pooling && newCapacity >= _options.PoolThreshold)
            {
                var memoryOwner = MemoryPool<byte>.Shared.Rent(newCapacity);

                if (_length > 0)
                    _memory.Span.Slice(0, _length).CopyTo(memoryOwner.Memory.Span);

                InternalDisposeMemory();

                _memory = memoryOwner!.Memory;
                _memoryOwner = memoryOwner;
            }
            else
            {
                var bytes = new byte[newCapacity];

                if (_length > 0)
                    _memory.Span.Slice(0, _length).CopyTo(bytes.AsSpan());

                InternalDisposeMemory();

                _memory = new Memory<byte>(bytes);
                _memoryOwner = null;
            }

            _length = capacity;
            _capacity = _memory.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalUpdateObjectLengthPrefix(int size)
        {
            ObjectSerialization? obj;

            for (obj = SerializationStack.Peek(); obj != null; obj = obj.Previous)
                obj.Update(this, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalEnsureSerializeCapacity(int size)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Serializer));

            if (_options.ReadOnly)
                throw new NotSupportedException("The serializer was initialized as read only.");

            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size), "size is negative");

            var newPosition = _position + size;

            if (newPosition < 0 || newPosition > _options.MaxCapacity)
                throw new InternalBufferOverflowException($"Invalid serialize length '{size}' on {DebuggerDisplay()}");

            if (newPosition > _length)
            {
                if (_stream == null)
                {
                    if (newPosition > _capacity)
                        InternalExpandMemory(newPosition);
                }
                else
                {
                    if (_stream.CanSeek)
                        _stream.SetLength(newPosition);
                }

                _length = newPosition;
            }

            if (PrefixObjectLengthActive)
                InternalUpdateObjectLengthPrefix(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalEnsureDeserializeCapacity(int size)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Serializer));

            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size), "size is negative");

            var newPosition = _position + size;

            if (newPosition < 0 || newPosition > Length)
                throw new EndOfStreamException($"Invalid deserialize length '{size}' on {DebuggerDisplay()}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool InternalTryEnsureDeserializeCapacity(int size)
        {
            if (_disposed)
                return false;

            if (size < 0)
                return false;

            var newPosition = _position + size;

            return newPosition >= 0 && newPosition <= Length;
        }
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //void EnsureCapacity(int size, SerializerOperation operation)
        //{
        //    if (_disposed)
        //        throw new ObjectDisposedException(nameof(Serializer));

        //    if (_stream != null)
        //        return;

        //    if (size < 0)
        //        throw new ArgumentOutOfRangeException(nameof(size));

        //    if (operation == SerializerOperation.Serialize && _options.ReadOnly)
        //        throw new InvalidOperationException("The buffer is marked as ReadOnly.");

        //    var newPosition = _position + size;

        //    if (newPosition < 0 || newPosition > MaxCapacity)
        //        throw new BufferOverflowException($"Invalid size '{size}' on {DebuggerDisplay()}");

        //    if (newPosition > _length)
        //    {
        //        if (operation == SerializerOperation.Deserialize)
        //            throw new BufferOverflowException();
        //        else
        //        {
        //            if (newPosition <= _capacity)
        //                _length = newPosition;
        //            else
        //            {
        //                var newCapacity = _capacity + _capacity;
        //                newCapacity = newCapacity < 0 || newCapacity > MaxCapacity
        //                    ? MaxCapacity
        //                    : newCapacity > newPosition
        //                        ? newCapacity
        //                        : newPosition < 256
        //                            ? 256
        //                            : newPosition;

        //                if (_options.Pooling && newCapacity >= _options.PoolThreshold)
        //                {
        //                    var memoryOwner = MemoryPool<byte>.Shared.Rent(newCapacity);

        //                    if (_length > 0)
        //                        _memory.Span.Slice(0, _length).CopyTo(memoryOwner.Memory.Span);

        //                    DisposeMemory();

        //                    _memory = memoryOwner!.Memory;
        //                    _memoryOwner = memoryOwner;
        //                }
        //                else
        //                {
        //                    var bytes = new byte[newCapacity];

        //                    if (_length > 0)
        //                        _memory.Span.Slice(0, _length).CopyTo(bytes.AsSpan());

        //                    DisposeMemory();

        //                    _memory = new Memory<byte>(bytes);
        //                    _memoryOwner = null;
        //                }

        //                _length = newPosition;
        //                _capacity = _memory.Length;
        //            }
        //        }
        //    }

        //    if (PrefixObjectLengthActive && operation == SerializerOperation.Serialize)
        //    {
        //        ObjectSerialization? obj;

        //        for (obj = SerializationStack.Peek(); obj != null; obj = obj.Previous)
        //            obj.Update(this, size);
        //    }
        //}

        #endregion Private

        #endregion Core
    }
}
