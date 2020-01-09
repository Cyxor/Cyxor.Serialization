using System;
using System.Linq;
using System.Reflection;

namespace Cyxor.Serialization
{
    using Extensions;

    internal static class SerializerDelegateCache
    {
        #region Serialization

        static readonly MethodInfo CreateActionDelegateMethodInfo
            = typeof(SerializerDelegateCache).GetMethodInfo(nameof(CreateActionDelegate), isStatic: true)!;

        static readonly ConcurrentCache<Type, Action<Serializer, object?>> SerializationCache
            = new ConcurrentCache<Type, Action<Serializer, object?>>();

        public static Action<Serializer, object?> GetSerializationMethod(Type type)
        {
            if (!SerializationCache.TryGetValue(type, out var action))
            {
                var serializationMethodInfo = Serializer.GetSerializerMethod(type, SerializerOperation.Serialize);

                var parameterType = serializationMethodInfo.GetParameters().First().ParameterType;

                action = (Action<Serializer, object?>)CreateActionDelegateMethodInfo.MakeGenericMethod(parameterType)
                    .Invoke(null, new object[] { serializationMethodInfo })!;

                _ = SerializationCache.TryAdd(type, action);
            }

            return action;
        }

        static Action<Serializer, object> CreateActionDelegate<T>(MethodInfo method)
        {
            var action = (Action<Serializer, T>)method.CreateDelegate(typeof(Action<Serializer, T>));
            return (serializer, t) => action(serializer, (T)t);
        }
        #endregion

        #region Deserialization
        static readonly MethodInfo CreateFuncDelegateMethodInfo
            = typeof(SerializerDelegateCache).GetMethodInfo(nameof(CreateFuncDelegate), isStatic: true)!;

        static readonly ConcurrentCache<Type, Func<Serializer, object?>> DeserializationCache
            = new ConcurrentCache<Type, Func<Serializer, object?>>();

        public static Func<Serializer, object?> GetDeserializationMethod(Type type)
        {
            if (!DeserializationCache.TryGetValue(type, out var func))
            {
                var deserializationMethodInfo = Serializer.GetSerializerMethod(type, SerializerOperation.Deserialize);

                var returnType = deserializationMethodInfo.ReturnType;

                func = (Func<Serializer, object?>)CreateFuncDelegateMethodInfo.MakeGenericMethod(returnType)
                    .Invoke(null, new object[] { deserializationMethodInfo })!;

                _ = DeserializationCache.TryAdd(type, func);
            }

            return func;
        }

        static Func<Serializer, object> CreateFuncDelegate<T>(MethodInfo method)
        {
            var func = (Func<Serializer, T>)method.CreateDelegate(typeof(Func<Serializer, T>));
            return (serializer) => func(serializer)!;
        }
        #endregion
    }
}