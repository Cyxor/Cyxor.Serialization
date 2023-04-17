namespace Cyxor.Serialization;

static partial class Utilities
{
    public class Unmanaged
    {
        public static Unmanaged Instance = new Unmanaged();
    }

    public static class TypeHelper
    {
        public static void Serialize<T>(T t) => throw new NotImplementedException(t?.ToString());

        public static T Serialization<T>() => throw new NotImplementedException();

        public static IEnumerable<T> IEnumerable<T>() => throw new NotImplementedException();

        public static IGrouping<TKey, TElement> IGrouping<TKey, TElement>() => throw new NotImplementedException();

        public static IEnumerable<KeyValuePair<TKey, TValue>> IEnumerableKeyValuePair<TKey, TValue>() =>
            throw new NotImplementedException();
    }

    public static class Bits
    {
        public static int RequiredBytes(int bits) => (bits + 7) / 8;

        public static unsafe int Required(int value) => Required((byte*)&value, sizeof(int));

        public static unsafe int Required(long value) => Required((byte*)&value, sizeof(long));

        public static unsafe int Required(char value) => Required((byte*)&value, sizeof(char));

        public static unsafe int Required(byte value) => Required((byte*)&value, sizeof(byte));

        public static unsafe int Required(uint value) => Required((byte*)&value, sizeof(uint));

        public static unsafe int Required(short value) => Required((byte*)&value, sizeof(short));

        public static unsafe int Required(float value) => Required((byte*)&value, sizeof(float));

        public static unsafe int Required(sbyte value) => Required((byte*)&value, sizeof(sbyte));

        public static unsafe int Required(ulong value) => Required((byte*)&value, sizeof(ulong));

        public static unsafe int Required(double value) => Required((byte*)&value, sizeof(double));

        public static unsafe int Required(ushort value) => Required((byte*)&value, sizeof(ushort));

        static unsafe int Required(byte* value, int size)
        {
            var count = 1;

            switch (size)
            {
                case sizeof(byte):
                    while ((*value >>= 1) != 0) count++;
                    break;
                case sizeof(short):
                    while ((*(ushort*)value >>= 1) != 0) count++;
                    break;
                case sizeof(int):
                    while ((*(uint*)value >>= 1) != 0) count++;
                    break;
                case sizeof(long):
                    while ((*(ulong*)value >>= 1) != 0) count++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), "Unsupported bits count");
            }

            return count;
        }
    }

    public static class Enum
    {
        //public static TEnum GetConstantOrDefault<TEnum>() where TEnum : struct, System.Enum
        //    => GetConstantOrDefault<TEnum>(value: default);

        public static TEnum ParseOrDefault<TEnum>(string? value = default)
            where TEnum : struct, System.Enum
        {
            if (value == default)
                return default;

            if (System.Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
                return result;

            var enumValues = System.Enum.GetValues(typeof(TEnum));

            //return enumValues.Length == 0 ? (default) : System.Enum.Parse<TEnum>(value, ignoreCase: true);
            return enumValues.Length == 0
                ? (default)
                : (TEnum)System.Enum.Parse(typeof(TEnum), value, ignoreCase: true);
        }
    }

    public static class HashCode
    {
        public static unsafe int GetFrom(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            fixed (char* chPtr = value) return GetFrom((byte*)chPtr, value.Length * 2);
        }

        public static unsafe int GetFrom(byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            fixed (byte* ptr = value) return GetFrom(ptr, value.Length);
        }

        public static unsafe int GetFrom(byte[] value, int offset, int count)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (offset == 0 && count == 0)
                return 0;

            if (value.Length - offset < count)
                throw new ArgumentException("Invalid value range");

            fixed (byte* ptr = value) return GetFrom(ptr + offset, count);
        }

        public static unsafe int GetFrom(byte* ptr, int length)
        {
            var numPtr = (int*)ptr;

            var num1 = 352654597;
            var num2 = num1;
            length = length == 1 ? 1 : length / 2;

            while (length > 0)
            {
                num1 = (num1 << 5) + num1 + (num1 >> 27) ^ *numPtr;

                if (length <= 2)
                    break;

                num2 = (num2 << 5) + num2 + (num2 >> 27) ^ numPtr[1];
                numPtr += 2;
                length -= 4;
            }

            return num1 + num2 * 1566083941;
        }
    }

    public static class ByteOrder
    {
        public static unsafe int Swap(int value) => *(int*)Swap((byte*)&value, sizeof(int));
        public static unsafe uint Swap(uint value) => *(uint*)Swap((byte*)&value, sizeof(uint));
        public static unsafe long Swap(long value) => *(long*)Swap((byte*)&value, sizeof(long));
        public static unsafe short Swap(short value) => *(short*)Swap((byte*)&value, sizeof(short));
        public static unsafe float Swap(float value) => *(float*)Swap((byte*)&value, sizeof(float));
        public static unsafe ulong Swap(ulong value) => *(ulong*)Swap((byte*)&value, sizeof(ulong));
        public static unsafe double Swap(double value) => *(double*)Swap((byte*)&value, sizeof(double));
        public static unsafe ushort Swap(ushort value) => *(ushort*)Swap((byte*)&value, sizeof(ushort));
        public static unsafe decimal Swap(decimal value) => *(ushort*)Swap((byte*)&value, sizeof(ushort));

        static unsafe byte* Swap(byte* value, int size)
        {
            if (size == sizeof(ushort))
            {
                *(ushort*)value = (ushort)((*(ushort*)value >> 8) | (*(ushort*)value << 8));
            }
            else if (size == sizeof(uint))
            {
                var uint32 = *(uint*)value;
                *(uint*)value = (uint32 >> 24)
                | ((uint32 & 0x00ff0000) >> 8)
                | ((uint32 & 0x0000ff00) << 8)
                | (uint32 << 24);
            }
            else if (size == sizeof(ulong))
            {
                var uint64 = *(ulong*)value;
                *(ulong*)value = (uint64 >> 56)
                | ((uint64 & 0x00ff000000000000L) >> 40)
                | ((uint64 & 0x0000ff0000000000L) >> 24)
                | ((uint64 & 0x000000ff00000000L) >> 8)
                | ((uint64 & 0x00000000ff000000L) << 8)
                | ((uint64 & 0x0000000000ff0000L) << 24)
                | ((uint64 & 0x000000000000ff00L) << 40)
                | (uint64 << 56);
            }
            else if (size == sizeof(decimal))
            {
                for (var i = 0; i < sizeof(decimal); i++)
                {
                    var tmp = value[sizeof(decimal) - 1 - i];
                    value[sizeof(decimal) - 1 - i] = value[i];
                    value[i] = tmp;
                }
            }
            else
                throw new ArgumentOutOfRangeException(nameof(size));

            return value;
        }
    }

    public static class Converter
    {
        public static byte[] FromHexString(string value)
        {
            var length = value.Length;
            var bytes = new byte[(length + 1) / 3];

            static int CharConvert(int @char) => @char - (@char > 0x60 ? 0x57 : @char > 0x40 ? 0x37 : 0x30);

            for (int i = 0, j = 0; i < length; i += 3, ++j)
                bytes[j] = (byte)((CharConvert(value[i]) << 4) + CharConvert(value[i + 1]));

            return bytes;
        }

        public static string ToHexString(byte[] value) => BitConverter.ToString(value);
    }

    public static class Reflection
    {
        public static TAttribute? GetCustomAssemblyAttribute<TAttribute>(Type type)
            where TAttribute : Attribute => type.Assembly.GetCustomAttribute<TAttribute>();
    }

    public static class EncodedInteger
    {
        public const int OneByteCap = 128;
        public const int TwoBytesCap = 16384;
        public const int ThreeBytesCap = 2097152;
        public const int FourBytesCap = 268435456;

        public static int RequiredBytes(short value) => RequiredBytes((ulong)((value << 1) ^ (value >> 15)));

        public static int RequiredBytes(int value) => RequiredBytes((uint)value);

        public static int RequiredBytes(long value) => RequiredBytes((ulong)((value << 1) ^ (value >> 63)));

        public static int RequiredBytes(ushort value) => RequiredBytes((ulong)value);

        public static int RequiredBytes(uint value) => RequiredBytes((ulong)value);

        public static int RequiredBytes(ulong value)
        {
            var bytes = (byte)0;

            while (value >= 128)
            {
                bytes++;
                value >>= 7;
            }

            return bytes + 1;
        }
    }
}
