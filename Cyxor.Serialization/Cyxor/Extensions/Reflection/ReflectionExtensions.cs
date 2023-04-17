using System.Linq;

namespace Cyxor.Extensions;

public static partial class ReflectionExtensions
{
    public static bool IsInterfaceImplemented<T>(this Type type) => IsInterfaceImplemented(type, typeof(T));

    public static bool IsInterfaceImplemented(this Type type, Type interfaceType) =>
        interfaceType.IsInterface
&& (type == interfaceType
|| type.IsInterface
                        && interfaceType.IsGenericTypeDefinition
                        && type.GetGenericTypeDefinition() == interfaceType
|| type.GetInterfaces()
                                    .Any(
                                        p =>
                                            interfaceType.IsGenericTypeDefinition
                                                ? p.IsGenericType && p.GetGenericTypeDefinition() == interfaceType
                                                : p == interfaceType
                                    ));
}
