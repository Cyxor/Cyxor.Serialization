using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#if !NET20
using System.Linq.Expressions;
#endif

#if !NET20 && !NET35 && !NET40
using System.Runtime.CompilerServices;
#endif

#if !NET20 && !NET35 && !NETSTANDARD1_0
using System.Collections.Concurrent;
#else
using System.Threading;
#endif

namespace Cyxor.Serialization
{
    using Extensions;

    partial class SerialStream
    {
        static partial class Reflector
        {
            internal static ConcurrentCache<Type, TypeData> TypesCache = new ConcurrentCache<Type, TypeData>();
            internal static ConcurrentCache<Type, bool> KnownTypesCache = new ConcurrentCache<Type, bool>();

            static bool ShouldSerializeField(FieldInfo field)
            {
                if (field.IsDefined(typeof(CyxorIgnoreAttribute), inherit: false))
                    return false;

                if (field.IsStatic && field.IsInitOnly)
                    return false;

                if (field.Name[0] != '<')
                    return true;

#pragma warning disable IDE0057 // Substring can be simplified
                var propertyName = field.Name.Substring(1, field.Name.IndexOf('>', StringComparison.Ordinal) - 1);
#pragma warning restore IDE0057 // Substring can be simplified

                var property = field.DeclaringType!.GetProperty(propertyName)!;

                return ShouldSerializeProperty(property);
            }

            static bool ShouldSerializeProperty(PropertyInfo property)
                => !property.IsDefined(typeof(CyxorIgnoreAttribute), inherit: false);

            public static bool IsKnownType(Type type)
            {
                if (KnownTypesCache.TryGetValue(type, out var result))
                    return result;

                result =
                    type.IsArray ||
                    type == typeof(Uri) ||
                    type == typeof(Guid) ||
                    type == typeof(string) ||
                    type == typeof(decimal) ||
                    type == typeof(TimeSpan) ||
                    type == typeof(DateTime) ||
                    type == typeof(SerialStream) ||
                    type == typeof(MemoryStream) ||
                    type == typeof(BitSerializer) ||
                    type.GetTypeInfo().IsEnum ||
                    type.GetTypeInfo().IsPrimitive ||
                    type == typeof(DateTimeOffset) ||
                    type.IsInterfaceImplemented<IEnumerable>();

                _ = KnownTypesCache.TryAdd(type, result);

                return result;
            }
        }
    }
}