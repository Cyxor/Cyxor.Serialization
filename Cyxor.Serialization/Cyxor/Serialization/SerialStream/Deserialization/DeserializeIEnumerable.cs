using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Cyxor.Serialization
{
    partial class SerialStream
    {
        #region Array

        T[] InternalDeserializeArray<T>(int count)
        {
            var type = typeof(T);

            if (type == typeof(byte))
            {
                var byteArray = DeserializeBytes(count);
                var tArray = byteArray as T[];

                if (tArray == default)
                    throw new InvalidOperationException(Utilities.ResourceStrings.UnableToCastByteArrayToTArray(type.Name));

                return tArray;
            }
            else if (type == typeof(char))
            {
                var charArray = DeserializeChars(count);
                var tArray = charArray as T[];

                if (tArray == default)
                    throw new InvalidOperationException(Utilities.ResourceStrings.UnableToCastCharArrayToTArray(type.Name));

                return tArray;
            }

            var array = new T[count];

            for (var i = 0; i < count; i++)
            {
                var value = DeserializeObject(type);
                array[i] = value == default ? default : (T)value;
            }

            return array;
        }

        public T[] DeserializeArray<T>()
        {
            var count = DeserializeOp();

            return count == -1 ? throw new InvalidOperationException(Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(typeof(T[]).Name))
                : count == 0 ? Utilities.Array.Empty<T>()
                : InternalDeserializeArray<T>(count);
        }

        public T[]? DeserializeNullableArray<T>()
        {
            var count = DeserializeOp();

            return count == -1 ? default
                : count == 0 ? Utilities.Array.Empty<T>()
                : InternalDeserializeArray<T>(count);
        }

        public T[] ToArray<T>()
        {
            position = 0;
            var array = DeserializeArray<T>();

            if (position != length)
                throw new InvalidOperationException(Utilities.ResourceStrings.TheWholeSerialStreamContentIsNotAnObjectOfType(typeof(T).Name));

            return array;
        }

        public T[]? ToNullableArray<T>()
        {
            position = 0;
            var array = DeserializeNullableArray<T>();

            if (position != length)
                throw new InvalidOperationException(Utilities.ResourceStrings.TheWholeSerialStreamContentIsNotAnObjectOfType(typeof(T).Name));

            return array;
        }

        #endregion Array

        #region GenericCollection

        public T InternalDeserializeGenericCollection<T>() where T : class
        {
            var type = typeof(T);
            var result = DeserializeObject(type);
            var obj = Activator.CreateInstance(type, result) as T;

            if (obj == default)
                throw new InvalidOperationException(Utilities.ResourceStrings.CantCreateInstanceOfType(type.Name));

            return obj;
        }

        public T? InternalDeserializeNullableGenericCollection<T>() where T : class
        {
            var type = typeof(T);
            var result = DeserializeNullableObject(type);

            if (result == default)
                return default;

            var obj = Activator.CreateInstance(type, result) as T;

            if (obj == default)
                throw new InvalidOperationException(Utilities.ResourceStrings.CantCreateInstanceOfType(type.Name));

            return obj;
        }

        #endregion GenericCollection

        #region Collection

        // TODO: Tests if return tIEnumerable! is correct and update Array
        IEnumerable<T> InternalDeserializeIEnumerable<T>(int count)
        {
            var type = typeof(T);

            if (type == typeof(byte))
            {
                var byteArray = DeserializeBytes(count);
                var tIEnumerable = byteArray.ToList() as IEnumerable<T>;

                return tIEnumerable!;
            }
            else if (type == typeof(char))
            {
                var charArray = DeserializeChars(count);
                var tIEnumerable = charArray.ToList() as IEnumerable<T>;

                return tIEnumerable!;
            }

            var list = new List<T>(capacity: count);

            for (var i = 0; i < count; i++)
            {
                var value = DeserializeObject(type);
                list.Add(value == default ? default : (T)value);
            }

            return list;
        }

        public IEnumerable<T> DeserializeIEnumerable<T>()
        {
            var count = DeserializeOp();

            return count == -1 ? throw new InvalidOperationException(Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(typeof(IEnumerable<T>).Name))
                : count == 0 ? new List<T>()
                : InternalDeserializeIEnumerable<T>(count);
        }

        public IEnumerable<T>? DeserializeNullableIEnumerable<T>()
        {
            var count = DeserializeOp();

            return count == -1 ? default
                : count == 0 ? new List<T>()
                : InternalDeserializeIEnumerable<T>(count);
        }

        public T DeserializeCollection<T>() where T : class, ICollection
            => InternalDeserializeGenericCollection<T>();

        public T? DeserializeNullableCollection<T>() where T : class, ICollection
            => InternalDeserializeNullableGenericCollection<T>();

        public IEnumerable<T> ToIEnumerable<T>()
        {
            position = 0;
            var items = DeserializeIEnumerable<T>();

            if (position != length)
                throw new InvalidOperationException(Utilities.ResourceStrings.TheWholeSerialStreamContentIsNotAnObjectOfType(typeof(T).Name));

            return items;
        }

        public IEnumerable<T>? ToNullableIEnumerable<T>()
        {
            position = 0;
            var items = DeserializeNullableIEnumerable<T>();

            if (position != length)
                throw new InvalidOperationException(Utilities.ResourceStrings.TheWholeSerialStreamContentIsNotAnObjectOfType(typeof(T).Name));

            return items;
        }

        #endregion Collection

        #region Dictionary

        IEnumerable<KeyValuePair<TKey, TValue>> InternalDeserializeIEnumerable<TKey, TValue>(int count)
            where TKey : notnull
        {
            var dictionary = new Dictionary<TKey, TValue>(capacity: count);

            for (var i = 0; i < count; i++)
            {
                var key = DeserializeObject(typeof(TKey));

                if (key == default)
                    throw new InvalidOperationException(Utilities.ResourceStrings.NullKeyWhenDeserializingDictionary(typeof(TKey).Name, typeof(TValue).Name));

                var value = DeserializeNullableObject(typeof(TValue));

                dictionary.Add((TKey)key, value == default ? default : (TValue)value);
            }

            return dictionary;
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> DeserializeIEnumerable<TKey, TValue>()
            where TKey : notnull
        {
            var count = DeserializeOp();

            return count == -1 ? throw new InvalidOperationException(Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(typeof(IEnumerable<KeyValuePair<TKey, TValue>>).Name))
                : count == 0 ? new Dictionary<TKey, TValue>()
                : InternalDeserializeIEnumerable<TKey, TValue>(count);
        }

        public IEnumerable<KeyValuePair<TKey, TValue>>? DeserializeNullableIEnumerable<TKey, TValue>()
            where TKey : notnull
        {
            var count = DeserializeOp();

            return count == -1 ? default
                : count == 0 ? new Dictionary<TKey, TValue>()
                : InternalDeserializeIEnumerable<TKey, TValue>(count);
        }

        public T DeserializeDictionary<T>() where T : class, IDictionary
            => InternalDeserializeGenericCollection<T>();

        public T? DeserializeNullableDictionary<T>() where T : class, IDictionary
            => InternalDeserializeNullableGenericCollection<T>();

        public IEnumerable<KeyValuePair<TKey, TValue>> ToIEnumerable<TKey, TValue>()
            where TKey : notnull
        {
            position = 0;
            var items = DeserializeIEnumerable<TKey, TValue>();

            if (position != length)
            {
                var keyTypeName = typeof(TKey).Name;
                var valueTypeName = typeof(TValue).Name;
                var typeName = $"IEnumerable<KeyValuePair<{keyTypeName}, {valueTypeName}>>";

                throw new InvalidOperationException(Utilities.ResourceStrings.TheWholeSerialStreamContentIsNotAnObjectOfType(typeName));
            }

            return items;
        }

        public IEnumerable<KeyValuePair<TKey, TValue>>? ToNullableIEnumerable<TKey, TValue>()
            where TKey : notnull
        {
            position = 0;
            var items = DeserializeNullableIEnumerable<TKey, TValue>();

            if (position != length)
            {
                var keyTypeName = typeof(TKey).Name;
                var valueTypeName = typeof(TValue).Name;
                var typeName = $"IEnumerable<KeyValuePair<{keyTypeName}, {valueTypeName}>>";

                throw new InvalidOperationException(Utilities.ResourceStrings.TheWholeSerialStreamContentIsNotAnObjectOfType(typeName));
            }

            return items;
        }

        #endregion Dictionary

        #region Grouping

        public IGrouping<TKey, TElement> DeserializeIGrouping<TKey, TElement>()
            where TKey : notnull
        {
            var grouping = DeserializeNullableIGrouping<TKey, TElement>();

            if (grouping != default)
                return grouping;

            throw new InvalidOperationException(Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(typeof(IGrouping<TKey, TElement>).Name));
        }

        public IGrouping<TKey, TElement>? DeserializeNullableIGrouping<TKey, TElement>()
            where TKey : notnull
        {
            var op = DeserializeByte();

            if (op == 0)
                return default;

            if (op != 1)
                throw new InvalidOperationException("Incorrect format when deserializing IGrouping of type ...");

            var key = DeserializeObject<TKey>();

            var elements = DeserializeNullableIEnumerable<TElement>();

            if (elements != default)
                return elements.GroupBy(p => key).Single();

            elements = new List<TElement>();
            return elements.GroupBy(p => key).Single();
        }

        public IGrouping<TKey, TElement> ToIGrouping<TKey, TElement>()
            where TKey : notnull
        {
            position = 0;
            var items = DeserializeIGrouping<TKey, TElement>();

            if (position != length)
            {
                var keyTypeName = typeof(TKey).Name;
                var valueTypeName = typeof(TElement).Name;
                var typeName = $"IGrouping<{keyTypeName}, {valueTypeName}>";

                throw new InvalidOperationException(Utilities.ResourceStrings.TheWholeSerialStreamContentIsNotAnObjectOfType(typeName));
            }

            return items;
        }

        public IGrouping<TKey, TElement>? ToNullableIGrouping<TKey, TElement>()
            where TKey : notnull
        {
            position = 0;
            var items = DeserializeNullableIGrouping<TKey, TElement>();

            if (position != length)
            {
                var keyTypeName = typeof(TKey).Name;
                var valueTypeName = typeof(TElement).Name;
                var typeName = $"IGrouping<{keyTypeName}, {valueTypeName}>";

                throw new InvalidOperationException(Utilities.ResourceStrings.TheWholeSerialStreamContentIsNotAnObjectOfType(typeName));
            }

            return items;
        }

        #endregion Grouping
    }
}