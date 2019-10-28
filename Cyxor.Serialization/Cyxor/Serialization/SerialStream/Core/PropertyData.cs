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

    partial class SerialStream
    {
        static partial class Reflector
        {
            internal sealed class PropertyData : IComparable<PropertyData>, IEquatable<PropertyData>
            {
                readonly int HashCode;

                public readonly string Name;
                public readonly PropertyInfo PropertyInfo;
                public readonly bool ShouldSerialize;
                public readonly bool NeedChangeCollection;

                readonly Func<object, object[]?, object> GetValueDelegate;
                readonly Action<object, object?, object[]?> SetValueDelegate;

                public override int GetHashCode() => HashCode;

                public object GetValue(object obj, object[]? index) => GetValueDelegate(obj, index);
                public void SetValue(object obj, object? value, object[]? index) => SetValueDelegate(obj, value, index);

                public int CompareTo(PropertyData other)
                    => string.Compare(Name, other.Name, StringComparison.Ordinal);

                public bool Equals(PropertyData other)
                    => CompareTo(other) == 0;

                public override bool Equals(object? obj)
                {
                    if (obj == default)
                        return false;

                    var propertyData = obj as PropertyData;

                    return propertyData == default ? false : Equals(propertyData);
                }

#if !NET20
                private static void Map<T>(out T dest, T src)
                    => dest = src;

                static readonly MethodInfo MapMethodInfo = typeof(PropertyData).GetMethodInfo(nameof(Map), isStatic: true)!;
#endif

                public PropertyData(PropertyInfo propertyInfo, bool shouldSerialize)
                {
                    PropertyInfo = propertyInfo;
                    Name = PropertyInfo.Name;
                    ShouldSerialize = shouldSerialize;

                    if (PropertyInfo.PropertyType.FullName == default)
                        throw new InvalidOperationException(Utilities.ResourceStrings.CyxorInternalException);

                    if (PropertyInfo.PropertyType.GetTypeInfo().IsGenericType)
                        if (!PropertyInfo.PropertyType.GetTypeInfo().IsInterface)
                            if (PropertyInfo.PropertyType.IsInterfaceImplemented<IEnumerable>())
                                if (!PropertyInfo.PropertyType.FullName.StartsWith($"{typeof(List<>).Namespace}.{typeof(List<>).Name}", StringComparison.Ordinal))
                                    NeedChangeCollection = true;

                    HashCode = Utilities.HashCode.GetFrom(propertyInfo.Name);
#if NET20
                    GetValueDelegate = PropertyInfo.GetValue;
                    SetValueDelegate = PropertyInfo.SetValue;
#else
                    var mapMethodInfo = MapMethodInfo.MakeGenericMethod(PropertyInfo.PropertyType);

                    var valueParameter = Expression.Parameter(typeof(object), "value");
                    var indexParameter = Expression.Parameter(typeof(object[]), "index");
                    var objectParameter = Expression.Parameter(typeof(object), "object");

                    var valueConverted = Expression.Convert(valueParameter, PropertyInfo.PropertyType);
                    var objectConverted = Expression.Convert(objectParameter, PropertyInfo.DeclaringType);

                    var propertyExpression = default(MemberExpression);

                    var memberExpression = ((Func<Expression, MemberExpression>)
                        (p => propertyExpression = Expression.Property(p, PropertyInfo)))(objectConverted);

                    var getterExpression = PropertyInfo.PropertyType.GetTypeInfo().IsValueType ?
                        Expression.Convert(memberExpression, typeof(object)) : (Expression)memberExpression;

                    var setterExpression = ((Func<Expression, Expression, MethodCallExpression>)
                        ((p, value) => Expression.Call(mapMethodInfo, propertyExpression, value)))(objectConverted, valueConverted);

                    GetValueDelegate = Expression.Lambda<Func<object, object[]?, object>>(getterExpression, objectParameter, indexParameter).Compile();

                    SetValueDelegate = Expression.Lambda<Action<object, object?, object[]?>>(setterExpression, objectParameter, valueParameter, indexParameter).Compile();
#endif
                }
            }
        }
    }
}