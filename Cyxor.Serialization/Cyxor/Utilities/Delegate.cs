using System;
using System.Linq;
using System.Reflection;

namespace Cyxor.Serialization
{
    using Extensions;

    internal static class Delegate
    {
        #region Func
        static readonly MethodInfo CreateFuncMethodInfo = typeof(Delegate).GetMethodInfo(nameof(CreateFunc), isStatic: true)!;

        static Delegate()
        {
            var x = CreateFuncMethodInfo;
            var y = typeof(Delegate).GetMethodsInfo(isStatic: true);
            var z = typeof(Delegate).GetMethodsInfo();
            var w = typeof(Delegate).GetMethodsInfo(isStatic: false);

            if (x == null)
                CreateFuncMethodInfo = null;
        }

        static readonly ConcurrentCache<Type, Func<SerialStream, object?>> FuncDelegateCache = new ConcurrentCache<Type, Func<SerialStream, object?>>();

        public static Func<SerialStream, object?> GetFunc(Type type)
        {
            if (!FuncDelegateCache.TryGetValue(type, out var func))
                _ = FuncDelegateCache.TryAdd(type, func = GetFunc<SerialStream>(SerialStream.GetSerializationMethod(type, SerializerOperation.Deserialize)));

            return func;
        }

        static Func<T, object?> GetFunc<T>(MethodInfo methodInfo) where T : class
            => (Func<T, object?>)CreateFuncMethodInfo.MakeGenericMethod(typeof(T), methodInfo.ReturnType).Invoke(default, new object[] { methodInfo })!;

        static Func<TTarget, object> CreateFunc<TTarget, TReturn>(MethodInfo method) where TTarget : class
        {
#if NET20 || NET35 || NET40
            var func = (Func<TTarget, TReturn>)System.Delegate.CreateDelegate(typeof(Func<TTarget, TReturn>), method);
#else
            var func = (Func<TTarget, TReturn>)method.CreateDelegate(typeof(Func<TTarget, TReturn>));
#endif
            return (target) => func(target)!;
        }
        #endregion

        #region Action

        static readonly MethodInfo CreateActionMethodInfo = typeof(Delegate).GetMethodInfo(nameof(CreateAction), isStatic: true);

        static readonly ConcurrentCache<Type, Action<SerialStream, object?>> ActionDelegateCache = new ConcurrentCache<Type, Action<SerialStream, object?>>();

        public static Action<SerialStream, object?> GetAction(Type type)
        {
            if (!ActionDelegateCache.TryGetValue(type, out var action))
                _ = ActionDelegateCache.TryAdd(type, action = GetAction<SerialStream>(SerialStream.GetSerializationMethod(type, SerializerOperation.Serialize)));

            return action;
        }

        static Action<T, object?> GetAction<T>(MethodInfo methodInfo) where T : class
            => (Action<T, object?>)CreateActionMethodInfo.MakeGenericMethod
                (typeof(T), methodInfo.GetParameters().First().ParameterType)
                .Invoke(default, new object[] { methodInfo })!;

        static Action<TTarget, object> CreateAction<TTarget, TParam>(MethodInfo method) where TTarget : class
        {
#if NET20 || NET35 || NET40
            var action = (Action<TTarget, TParam>)System.Delegate.CreateDelegate(typeof(Action<TTarget, TParam>), method);
#else
            var action = (Action<TTarget, TParam>)method.CreateDelegate(typeof(Action<TTarget, TParam>));
#endif
            return (target, param) => action(target, (TParam)param);
        }
        #endregion
    }
}