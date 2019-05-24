using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Cyxor.Serialization
{
    class Utilities
    {
        protected Utilities() { }

        public static class Bits
        {
            public static int RequiredBytes(int bits)
                => (bits + 7) / 8;

            public static unsafe int Required(int value)
                => Required((byte*)&value, sizeof(int));

            public static unsafe int Required(long value)
                => Required((byte*)&value, sizeof(long));

            public static unsafe int Required(char value)
                => Required((byte*)&value, sizeof(char));

            public static unsafe int Required(byte value)
                => Required((byte*)&value, sizeof(byte));

            public static unsafe int Required(uint value)
                => Required((byte*)&value, sizeof(uint));

            public static unsafe int Required(short value)
                => Required((byte*)&value, sizeof(short));

            public static unsafe int Required(float value)
                => Required((byte*)&value, sizeof(float));

            public static unsafe int Required(sbyte value)
                => Required((byte*)&value, sizeof(sbyte));

            public static unsafe int Required(ulong value)
                => Required((byte*)&value, sizeof(ulong));

            public static unsafe int Required(double value)
                => Required((byte*)&value, sizeof(double));

            public static unsafe int Required(ushort value)
                => Required((byte*)&value, sizeof(ushort));

            static unsafe int Required(byte* value, int size)
            {
                var count = 1;

                switch (size)
                {
                    case sizeof(byte): while ((*(byte*)value >>= 1) != 0) count++; break;
                    case sizeof(short): while ((*(ushort*)value >>= 1) != 0) count++; break;
                    case sizeof(int): while ((*(uint*)value >>= 1) != 0) count++; break;
                    case sizeof(long): while ((*(ulong*)value >>= 1) != 0) count++; break;

                    default: throw new ArgumentOutOfRangeException("bits", "Unsupported bits count");
                }

                return count;
            }
        }

        public static class Enum
        {
            public static TEnum GetConstantOrDefault<TEnum>() where TEnum : struct
                => GetConstantOrDefault<TEnum>(nameOrId: null);

#if NULLER
            public static TEnum GetConstantOrDefault<TEnum>(string? nameOrId) where TEnum : struct
#else
            public static TEnum GetConstantOrDefault<TEnum>(string nameOrId) where TEnum : struct
#endif
                => nameOrId == null ? System.Enum.GetValues(typeof(TEnum)).Length == 0 ? default :
                (TEnum)System.Enum.GetValues(typeof(TEnum)).GetValue(0) :
                (TEnum)System.Enum.Parse(typeof(TEnum), nameOrId, ignoreCase: true);
        }

        public static class Memory
        {
            public static unsafe void Memcpy(byte* source, byte* destination, int bytesToCopy)
#if !NET20 && !NET35 && !NET40 && !NET45 && !NETSTANDARD1_0
                => System.Buffer.MemoryCopy(source, destination, bytesToCopy, bytesToCopy);
#else
            {
                if (bytesToCopy >= 16)
                {
                    do
                    {
                        *(int*)destination = *(int*)source;
                        *(int*)(destination + 4) = *(int*)(source + 4);
                        *(int*)(destination + 8) = *(int*)(source + 8);
                        *(int*)(destination + 12) = *(int*)(source + 12);

                        destination += 16;
                        source += 16;
                    }
                    while ((bytesToCopy -= 16) >= 16);
                }

                if (bytesToCopy <= 0)
                    return;

                if ((bytesToCopy & 8) != 0)
                {
                    *(int*)destination = *(int*)source;
                    *(int*)(destination + 4) = *(int*)(source + 4);

                    destination += 8;
                    source += 8;
                }

                if ((bytesToCopy & 4) != 0)
                {
                    *(int*)destination = *(int*)source;

                    destination += 4;
                    source += 4;
                }

                if ((bytesToCopy & 2) != 0)
                {
                    *(short*)destination = *(short*)source;

                    destination += 2;
                    source += 2;
                }

                if ((bytesToCopy & 1) == 0)
                    return;

                *destination++ = *source++;
            }
#endif

            public static unsafe void Wstrcpy(char* source, char* destination, int charsToCopy)
#if !NET20 && !NET35 && !NET40 && !NET45 && !NETSTANDARD1_0
                => System.Buffer.MemoryCopy(source, destination, charsToCopy * 2, charsToCopy * 2);
#else
            {
                if (charsToCopy <= 0)
                    return;

                if (((int)destination & 2) != 0)
                {
                    *destination = *source;

                    ++destination;
                    ++source;

                    --charsToCopy;
                }

                while (charsToCopy >= 8)
                {
                    *(int*)destination = (int)*(uint*)source;
                    *(int*)(destination + 2) = (int)*(uint*)(source + 2);
                    *(int*)(destination + 4) = (int)*(uint*)(source + 4);
                    *(int*)(destination + 6) = (int)*(uint*)(source + 6);

                    destination += 8;
                    source += 8;

                    charsToCopy -= 8;
                }

                if ((charsToCopy & 4) != 0)
                {
                    *(int*)destination = (int)*(uint*)source;
                    *(int*)(destination + 2) = (int)*(uint*)(source + 2);

                    destination += 4;
                    source += 4;
                }

                if ((charsToCopy & 2) != 0)
                {
                    *(int*)destination = (int)*(uint*)source;

                    destination += 2;
                    source += 2;
                }

                if ((charsToCopy & 1) == 0)
                    return;

                *destination = *source;
            }
#endif

            public static unsafe int Strlen(byte* ptr)
            {
                var bytePtr = ptr;

                while ((int)*bytePtr != 0)
                    ++bytePtr;

                return (int)(bytePtr - ptr);
            }

            public static unsafe int Wcslen(char* ptr)
            {
                var chPtr = ptr;

                while (((int)(uint)chPtr & 3) != 0 && (int)*chPtr != 0)
                    ++chPtr;

                if ((int)*chPtr != 0)
                    while (((int)*chPtr & (int)chPtr[1]) != 0 || (int)*chPtr != 0 && (int)chPtr[1] != 0)
                        chPtr += 2;

                while ((int)*chPtr != 0)
                    ++chPtr;

                return (int)(chPtr - ptr);
            }
        }

        public static class HashCode
        {
            public static unsafe int GetFrom(string value)
            {
                if (string.IsNullOrEmpty(value))
                    return 0;

                fixed (char* chPtr = value)
                    return GetFrom((byte*)chPtr, value.Length * 2);
            }

            public static unsafe int GetFrom(byte[] value)
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                fixed (byte* ptr = value)
                    return GetFrom(ptr, value.Length);
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

                fixed (byte* ptr = value)
                    return GetFrom(ptr + offset, count);
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
                    *(uint*)value = (uint32 >> 24) | ((uint32 & 0x00ff0000) >> 8) |
                                    ((uint32 & 0x0000ff00) << 8) | (uint32 << 24);
                }
                else if (size == sizeof(ulong))
                {
                    var uint64 = *(ulong*)value;
                    *(ulong*)value = (uint64 >> 56) | ((uint64 & 0x00ff000000000000L) >> 40) |
                                     ((uint64 & 0x0000ff0000000000L) >> 24) | ((uint64 & 0x000000ff00000000L) >> 8) |
                                     ((uint64 & 0x00000000ff000000L) << 8) | ((uint64 & 0x0000000000ff0000L) << 24) |
                                     ((uint64 & 0x000000000000ff00L) << 40) | (uint64 << 56);
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
                    throw new ArgumentOutOfRangeException("Incorrect swap bytes order data size");

                return value;
            }
        }

        public static class Converter
        {
            public static byte[] FromHexString(string value)
            {
                var length = value.Length;
                var bytes = new byte[(length + 1) / 3];

                int CharConvert(int @char) => @char - (@char > 0x60 ? 0x57 : @char > 0x40 ? 0x37 : 0x30);

                for (int i = 0, j = 0; i < length; i += 3, ++j)
                    bytes[j] = (byte)((CharConvert(value[i]) << 4) + CharConvert(value[i + 1]));

                return bytes;
            }

            public static string ToHexString(byte[] value)
                => BitConverter.ToString(value);
        }

        public static class Reflection
        {
#if NET40 || NET35 || NET20
            internal static BindingFlags GenericBindingFlags =
                BindingFlags.DeclaredOnly |
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Static |
                BindingFlags.Public;
#endif

            public static TAttribute GetCustomAssemblyAttribute<TAttribute>(Type type) where TAttribute : Attribute
#if NET20 || NET40 || NET35
                => (TAttribute)type.Assembly.GetCustomAttributes(typeof(TAttribute), inherit: false).FirstOrDefault();
#else
                => type.GetTypeInfo().Assembly.GetCustomAttribute<TAttribute>();
#endif

            public static FieldInfo GetDeclaredField(Type type, string name)
#if NET40 || NET35 || NET20
                => type.GetField(name, GenericBindingFlags);
#else
                => type.GetTypeInfo().GetDeclaredField(name);
#endif

            public static IEnumerable<FieldInfo> GetDeclaredFields(Type type)
#if NET40 || NET35 || NET20
                => type.GetFields(GenericBindingFlags);
#else
                => type.GetTypeInfo().DeclaredFields;
#endif

            public static IEnumerable<MethodInfo> GetDeclaredPublicMethods(Type type)
#if NET40 || NET35 || NET20
                => type.GetMethods(GenericBindingFlags);
#else
                => type.GetTypeInfo().DeclaredMethods.Where(m => m.IsPublic);
#endif

            public static MethodInfo ConfigSetMethod(Type type, string propertyName)
#if NET40 || NET35 || NET20
                => type.GetProperty(propertyName).GetSetMethod(nonPublic: false);
#else
                => type.GetRuntimeProperty(propertyName).SetMethod;
#endif

            public static Type[] GetGenericArguments(Type type)
#if NET40 || NET35 || NET20
                => type.GetGenericArguments();
#else
                => type.GetTypeInfo().GenericTypeArguments;
#endif

            public static ConstructorInfo GetConstructor(Type type, params Type[] parameters)
#if NET40 || NET35 || NET20
                => type.GetConstructor(parameters);
#else
                => type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c =>
                    c.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameters));
#endif

            public static bool HasConstructor(Type type, params Type[] parameters)
#if NET40 || NET35 || NET20
                => type.GetConstructor(parameters) != default;
#else
                => type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c =>
                    c.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameters)) != default;
#endif

            public static PropertyInfo GetAnyDeclaredProperty(Type type, string name)
#if NET40 || NET35 || NET20
                => type.GetProperty(name, GenericBindingFlags);
#else
                => type.GetTypeInfo().DeclaredProperties.SingleOrDefault(p => p.Name == name);
#endif
        }

        public static class EncodedInteger
        {
            public const int OneByteCap = 128;
            public const int TwoBytesCap = 16384;
            public const int ThreeBytesCap = 2097152;
            public const int FourBytesCap = 268435456;

            public static int RequiredBytes(short value)
                => RequiredBytes((ulong)((value << 1) ^ (value >> 15)));

            public static int RequiredBytes(int value)
                => RequiredBytes((uint)value);

            public static int RequiredBytes(long value)
                => RequiredBytes((ulong)((value << 1) ^ (value >> 63)));

            public static int RequiredBytes(ushort value)
                => RequiredBytes((ulong)value);

            public static int RequiredBytes(uint value)
                => RequiredBytes((ulong)value);

            public static int RequiredBytes(ulong value)
            {
                var bytes = (byte)0;

                while (value >= 0x80)
                {
                    bytes++;
                    value >>= 7;
                }

                return bytes + 1;
            }
        }

        internal static class ResourceStrings
        {
            internal const string

                ExceptionNegativeNumber = "Non-negative number required.",

                CyxorInternalException = "Cyxor internal exception.",

                ExceptionFormat = "Cyxor..{0}.{1}() : {2}",
                ExceptionFormat1 = "Cyxor..{0}.{1}({2}) : {3}",
                ExceptionFormat2 = "Cyxor..{0}.{1}({2}, {3}) : {4}",
                ExceptionFormat3 = "Cyxor..{0}.{1}({2}, {3}, {4}) : {5}",
                ExceptionFormat4 = "Cyxor..{0}.{1}({2}, {3}, {4}) : {5}",
                ExceptionMessageBufferDeserializeNumeric = "",
                ExceptionMessageBufferDeserializeObject = "Deserialization operation do not match format of bytes written in the Serialization process.";
        }
    }
}