using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Cyxor.Serialization
{
    partial class SerialStream
    {
        void InternalSerialize<T1, T2>(IEnumerable<T1>? value1, IEnumerable<KeyValuePair<T1, T2>>? value2)
        {
            var value = (IEnumerable?)value1 ?? value2;

            if (value == default)
            {
                Serialize((byte)0);
                return;
            }

            var count = 0;

            foreach (var item in value)
                count++;

            if (count == 0)
            {
                Serialize(ObjectProperties.EmptyMap);
                return;
            }

            if (value == value1)
            {
                if (typeof(T1) == typeof(byte))
                {
                    Serialize(value1.ToArray() as byte[]);
                    return;
                }
                else if (typeof(T1) == typeof(char))
                {
                    Serialize(value1.ToArray() as char[]);
                    return;
                }
            }

            SerializeOp(count);

            if (value == value1)
                foreach (var item in value1!)
                    TypeSerializeObject(item, typeof(T1), raw: false);
            else
                foreach (var item in value2!)
                {
                    TypeSerializeObject(item.Key, typeof(T1), raw: false);
                    TypeSerializeObject(item.Value, typeof(T2), raw: false);
                }
        }

        public void Serialize<T>(T[]? value)
            => InternalSerialize<T, T>(value, default);

        public void Serialize<T>(IEnumerable<T>? value)
            => InternalSerialize<T, T>(value, default);

        public void Serialize<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>>? value)
            where TKey : notnull
            => InternalSerialize<TKey, TValue>(default, value);

        public void Serialize<TKey, TElement>(IGrouping<TKey, TElement>? value)
            where TKey : notnull
        {
            if (value == default)
            {
                Serialize((byte)0);
                return;
            }

            Serialize((byte)1);

            Serialize(value.Key);

            InternalSerialize<TElement, TElement>(value, default);
        }
    }
}