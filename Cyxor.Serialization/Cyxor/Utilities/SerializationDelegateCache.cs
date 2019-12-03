using System;
using System.Linq;
using System.Reflection;

namespace Cyxor.Serialization
{
    using Extensions;

    internal static class SerializationDelegateCache
    {
        #region Serialization

        static readonly MethodInfo CreateActionDelegateMethodInfo
            = typeof(SerializationDelegateCache).GetMethodInfo(nameof(CreateActionDelegate), isStatic: true)!;

        static readonly ConcurrentCache<Type, Action<SerializationStream, object?>> SerializationCache
            = new ConcurrentCache<Type, Action<SerializationStream, object?>>();

        public static Action<SerializationStream, object?> GetSerializationMethod(Type type)
        {
            if (!SerializationCache.TryGetValue(type, out var action))
            {
                var serializationMethodInfo = SerializationStream.GetSerializationMethod(type, SerializerOperation.Serialize);
                _ = SerializationCache.TryAdd(type, action = GetAction<SerializationStream>(serializationMethodInfo));
            }

            return action;
        }

        static Action<T, object?> GetAction<T>(MethodInfo methodInfo) where T : class
            => (Action<T, object?>)CreateActionDelegateMethodInfo.MakeGenericMethod
                (typeof(T), methodInfo.GetParameters().First().ParameterType)
                .Invoke(default, new object[] { methodInfo })!;

        static Action<TTarget, object> CreateActionDelegate<TTarget, TParam>(MethodInfo method) where TTarget : class
        {
#if NET20 || NET35 || NET40
            var action = (Action<TTarget, TParam>)System.Delegate.CreateDelegate(typeof(Action<TTarget, TParam>), method);
#else
            var action = (Action<TTarget, TParam>)method.CreateDelegate(typeof(Action<TTarget, TParam>));
#endif
            return (target, param) => action(target, (TParam)param);
        }
        #endregion

        #region Deserialization
        static readonly MethodInfo CreateFuncDelegateMethodInfo
            = typeof(SerializationDelegateCache).GetMethodInfo(nameof(CreateFuncDelegate), isStatic: true)!;

        //static SerializationDelegateCache()
        //{
        //    var x = CreateFuncMethodInfo;
        //    var y = typeof(SerializationDelegateCache).GetMethodsInfo(staticMethods: true);
        //    var z = typeof(SerializationDelegateCache).GetMethodsInfo();
        //    var w = typeof(SerializationDelegateCache).GetMethodsInfo(staticMethods: false);

        //    if (x == null)
        //        CreateFuncMethodInfo = null;
        //}

        static readonly ConcurrentCache<Type, Func<SerializationStream, object?>> FunctionsCache = new ConcurrentCache<Type, Func<SerializationStream, object?>>();

        public static Func<SerializationStream, object?> GetFunc(Type type)
        {
            if (!FunctionsCache.TryGetValue(type, out var func))
                _ = FunctionsCache.TryAdd(type, func = GetFunc<SerializationStream>(SerializationStream.GetSerializationMethod(type, SerializerOperation.Deserialize)));

            return func;
        }

        static Func<T, object?> GetFunc<T>(MethodInfo methodInfo) where T : class
            => (Func<T, object?>)CreateFuncDelegateMethodInfo.MakeGenericMethod(typeof(T), methodInfo.ReturnType).Invoke(default, new object[] { methodInfo })!;

        static Func<TTarget, object> CreateFuncDelegate<TTarget, TReturn>(MethodInfo method) where TTarget : class
        {
#if NET20 || NET35 || NET40
            var func = (Func<TTarget, TReturn>)System.Delegate.CreateDelegate(typeof(Func<TTarget, TReturn>), method);
#else
            var func = (Func<TTarget, TReturn>)method.CreateDelegate(typeof(Func<TTarget, TReturn>));
#endif
            return (target) => func(target)!;
        }
        #endregion
    }
}