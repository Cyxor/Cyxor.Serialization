using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

#if !NET20
using System.Linq.Expressions;
#endif

namespace Cyxor.Serialization
{
    using Extensions;

    sealed class FieldData : IComparable<FieldData>, IEquatable<FieldData>
    {
        readonly int HashCode;

        public readonly string Name;
        public readonly FieldInfo FieldInfo;
        public readonly bool ShouldSerialize;
        public readonly bool NeedChangeCollection;
        public readonly Func<object, object> GetValueDelegate;
        public readonly Action<object, object?> SetValueDelegate;
        public readonly Func<Serializer, object?> DeserializeFunc;
        public readonly Action<Serializer, object?> SerializeAction;

        public override int GetHashCode()
            => HashCode;

        public int CompareTo(FieldData other)
            => string.Compare(Name, other.Name, StringComparison.Ordinal);

        public bool Equals(FieldData other)
            => CompareTo(other) == 0;

        public override bool Equals(object? obj)
        {
            if (obj == default)
                return false;

            var fieldData = obj as FieldData;

            return fieldData == default ? false : Equals(fieldData);
        }

#if !NET20
        private static void Map<T>(out T dest, T src)
            => dest = src;

        static readonly MethodInfo MapMethodInfo = typeof(FieldData).GetMethodInfo(nameof(Map), isStatic: true, isInherited: false) ?? throw new InvalidOperationException(Utilities.ResourceStrings.CyxorInternalException);
#endif

        public FieldData(FieldInfo fieldInfo, bool shouldSerialize)
        {
            FieldInfo = fieldInfo;
            Name = FieldInfo.Name;
            ShouldSerialize = shouldSerialize;

            SerializeAction = SerializerDelegateCache.GetSerializationMethod(FieldInfo.FieldType);
            DeserializeFunc = SerializerDelegateCache.GetDeserializationMethod(FieldInfo.FieldType);

            if (FieldInfo.FieldType.FullName == default)
                throw new InvalidOperationException(Utilities.ResourceStrings.CyxorInternalException);

            if (FieldInfo.FieldType.GetTypeInfo().IsGenericType)
                if (!FieldInfo.FieldType.GetTypeInfo().IsInterface)
                    if (FieldInfo.FieldType.IsInterfaceImplemented<IEnumerable>())
                        if (!FieldInfo.FieldType.FullName.StartsWith($"{typeof(List<>).Namespace}.{typeof(List<>).Name}", StringComparison.Ordinal))
                            NeedChangeCollection = true;

            HashCode = Utilities.HashCode.GetFrom(FieldInfo.Name);
#if NET20
            GetValueDelegate = FieldInfo.GetValue;
            SetValueDelegate = FieldInfo.SetValue;
#else
            var mapMethodInfo = MapMethodInfo.MakeGenericMethod(FieldInfo.FieldType);

            var valueParameter = Expression.Parameter(typeof(object), "value");
            var objectParameter = FieldInfo.IsStatic ? null : Expression.Parameter(typeof(object), "object");

            var valueConverted = Expression.Convert(valueParameter, FieldInfo.FieldType);
            var objectConverted = FieldInfo.IsStatic ? null : Expression.Convert(objectParameter, FieldInfo.DeclaringType);

            var fieldExpression = default(MemberExpression);

            var memberExpression = ((Func<Expression?, MemberExpression>)
                (p => fieldExpression = Expression.Field(p, FieldInfo)))(objectConverted);

            var getterExpression = FieldInfo.FieldType.GetTypeInfo().IsValueType
                ? Expression.Convert(memberExpression, typeof(object))
                : (Expression)memberExpression;

            var setterExpression = ((Func<Expression?, Expression, MethodCallExpression>)
                ((p, value) => Expression.Call(mapMethodInfo, fieldExpression, value)))(objectConverted, valueConverted);

            GetValueDelegate = Expression.Lambda<Func<object?, object>>(getterExpression, objectParameter).Compile();

            SetValueDelegate = Expression.Lambda<Action<object?, object?>>(setterExpression, objectParameter, valueParameter).Compile();
#endif
        }
    }
}