using System;

namespace Cyxor.Serialization
{
    partial class SerializationStream
    {
        void SerializeNumeric(ValueType value, int size, bool unsigned, bool floatingPoint = false, bool? littleEndian = default)
        {
            EnsureCapacity(size, SerializerOperation.Serialize);

            unsafe
            {
                fixed (byte* ptr = &buffer![position])
                {
                    var sizeError = false;

                    var isLittleEndian = littleEndian ?? BitConverter.IsLittleEndian;
                    var swap = BitConverter.IsLittleEndian && !isLittleEndian || !BitConverter.IsLittleEndian && isLittleEndian;

                    if (unsigned)
                        switch (size)
                        {
                            case sizeof(byte): *ptr = (byte)value; break;
                            case sizeof(ushort): *(ushort*)ptr = swap ? Utilities.ByteOrder.Swap((ushort)value) : (ushort)value; break;
                            case sizeof(uint): *(uint*)ptr = swap ? Utilities.ByteOrder.Swap((uint)value) : (uint)value; break;
                            case sizeof(ulong): *(ulong*)ptr = swap ? Utilities.ByteOrder.Swap((ulong)value) : (ulong)value; break;

                            default: sizeError = true; break;
                        }
                    else
                        switch (size)
                        {
                            case sizeof(sbyte): *(sbyte*)ptr = (sbyte)value; break;
                            case sizeof(short): *(short*)ptr = swap ? Utilities.ByteOrder.Swap((short)value) : (short)value; break;
                            case sizeof(int):
                            {
                                if (floatingPoint)
                                    *(float*)ptr = swap ? Utilities.ByteOrder.Swap((float)value) : (float)value;
                                else
                                    *(int*)ptr = swap ? Utilities.ByteOrder.Swap((int)value) : (int)value;

                                break;
                            }
                            case sizeof(long):
                            {
                                if (floatingPoint)
                                    *(double*)ptr = swap ? Utilities.ByteOrder.Swap((double)value) : (double)value;
                                else
                                    *(long*)ptr = swap ? Utilities.ByteOrder.Swap((long)value) : (long)value;

                                break;
                            }
                            case sizeof(decimal): *(decimal*)ptr = swap ? Utilities.ByteOrder.Swap((decimal)value) : (decimal)value; break;

                            default: sizeError = true; break;
                        }

                    if (sizeError)
                        throw new ArgumentException(Utilities.ResourceStrings.ExceptionMessageBufferDeserializeNumeric);
                }
            }

            position += size;
        }

        public void Serialize(bool value)
            => SerializeNumeric((byte)(value ? 1 : 0), sizeof(bool), unsigned: true);

        public void Serialize(byte value)
            => SerializeNumeric(value, sizeof(byte), unsigned: true);

        public void Serialize(short value)
            => SerializeNumeric(value, sizeof(short), unsigned: false);

        public void Serialize(short value, bool littleEndian)
            => SerializeNumeric(value, sizeof(short), unsigned: false, littleEndian);

        public void Serialize(float value)
            => SerializeNumeric(value, sizeof(float), unsigned: false, floatingPoint: true);

        public void Serialize(double value)
            => SerializeNumeric(value, sizeof(double), unsigned: false, floatingPoint: true);

        public void Serialize(decimal value)
            => SerializeNumeric(value, sizeof(decimal), unsigned: false, floatingPoint: true);

        public void Serialize(sbyte value)
            => SerializeNumeric(value, sizeof(sbyte), unsigned: false);

        public void Serialize(ushort value)
            => SerializeNumeric(value, sizeof(ushort), unsigned: true);

        public void Serialize(ushort value, bool littleEndian)
            => SerializeNumeric(value, sizeof(ushort), unsigned: true, littleEndian);

        public void Serialize(uint value)
            => SerializeNumeric(value, sizeof(uint), unsigned: true);

        public void Serialize(uint value, bool littleEndian)
            => SerializeNumeric(value, sizeof(uint), unsigned: true, littleEndian);

        public void Serialize(ulong value)
            => SerializeNumeric(value, sizeof(ulong), unsigned: true);

        public void Serialize(ulong value, bool littleEndian)
            => SerializeNumeric(value, sizeof(ulong), unsigned: true, littleEndian);

        public void Serialize(char value)
            => Serialize((ushort)value);

        public void Serialize(int value)
            => SerializeCompressedInt((uint)value);

        public void Serialize(int value, bool littleEndian)
            => SerializeNumeric(value, sizeof(int), unsigned: false, littleEndian);

        public void SerializeUncompressedInt32(int value)
            => SerializeNumeric(value, sizeof(int), unsigned: false);

        public void Serialize(long value)
            => SerializeCompressedInt((ulong)value);

        public void Serialize(long value, bool littleEndian)
            => SerializeNumeric(value, sizeof(long), unsigned: false, littleEndian);

        public void SerializeUncompressedInt64(long value)
            => SerializeNumeric(value, sizeof(long), unsigned: false);

        public void Serialize(Guid value)
            => SerializeRaw(value.ToByteArray());

        public void Serialize(BitSerializer value)
            => Serialize((long)value);

        public void Serialize(TimeSpan value)
            => Serialize(value.Ticks);

        public void Serialize(DateTime value)
            => Serialize(value.Ticks);

        public void Serialize(DateTimeOffset value)
        {
            Serialize(value.DateTime);
            Serialize(value.Offset);
        }

        public void SerializeEnum<T>(T value) where T: struct, Enum
            => Serialize(Convert.ToInt64(value, Culture));

        //public void Serialize(Enum value)
        //    => Serialize(Convert.ToInt64(value, Culture));
    }
}