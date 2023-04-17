namespace Cyxor.Serialization;

partial class Serializer
{
    void InternalSerialize<T1, T2, TKey>(
        IEnumerable<T1>? value1,
        IEnumerable<KeyValuePair<TKey, T2>>? value2,
        int count = -1
    )
        where TKey : notnull
    {
        if (value1 == null && value2 == null)
        {
            Serialize((byte)0);
            return;
        }

        if (count == -1)
        {
            count = 0;
            var value = value1 ?? (IEnumerable?)value2;

            foreach (var item in value!) count++;
        }

        if (count == 0)
        {
            Serialize(EmptyMap);
            return;
        }

        InternalSerializeSequenceHeader(count);

        if (value1 != null)
            SaveIEnumerable(this, value1, count);
        else if (value2 != null)
        {
            if (value2 is IDictionary<TKey, T2> dictionary)
            {
                SaveIEnumerable(this, dictionary.Keys, count);
                SaveIEnumerable(this, dictionary.Values, count);
            }
            else
                foreach (var item in value2)
                {
                    Serialize(item.Key);
                    Serialize(item.Value);
                }
        }

        static void SaveIEnumerable<T>(Serializer serializer, IEnumerable<T> items, int count)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                foreach (var item in items) serializer.Serialize(item);
            else
            {
                var tSize = Unsafe.SizeOf<T>();
                serializer.InternalEnsureSerializeCapacity(tSize * count);

                foreach (var item in items) if (tSize < IntPtr.Size * 2)
                        serializer.InternalSerializeUnmanagedUnconstrained(item);
                    else
                        serializer.InternalSerializeUnmanagedUnconstrained(in item);
            }
        }
    }

    [SerializerMethodIdentifier(SerializerMethodIdentifier.SerializeArray)]
    public void Serialize<T>(T[]? value)
    {
        var length = value?.Length ?? 0;

        if (value == null || length == 0 || RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            InternalSerialize<T, T, byte>(value, default, length);
        else
        {
            var size = Unsafe.SizeOf<T>();
            ref var bytesRef = ref Unsafe.As<T, byte>(ref value[0]);
            var span = MemoryMarshal.CreateReadOnlySpan(ref bytesRef, length * size);
            Serialize(span);
        }
    }

    [SerializerMethodIdentifier(SerializerMethodIdentifier.SerializeIEnumerable)]
    public void Serialize<T>(IEnumerable<T>? value)
    {
        if (value is T[] array)
            Serialize(array);
        else
            InternalSerialize<T, T, byte>(value, default);
    }

    [SerializerMethodIdentifier(SerializerMethodIdentifier.SerializeIDictionary)]
    public void Serialize<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>>? value)
        where TKey : notnull => InternalSerialize<TValue, TValue, TKey>(default, value);

    [SerializerMethodIdentifier(SerializerMethodIdentifier.SerializeIGrouping)]
    public void Serialize<TKey, TElement>(IGrouping<TKey, TElement>? value)
        where TKey : notnull
    {
        if (value == null)
            Serialize((byte)0);
        else
        {
            Serialize((byte)1);
            Serialize(value.Key);
            InternalSerialize<TElement, TElement, byte>(value, default);
        }
    }
}
