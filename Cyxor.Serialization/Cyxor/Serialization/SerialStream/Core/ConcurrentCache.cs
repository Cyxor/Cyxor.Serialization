using System;

#if !NET20 && !NET35 && !NETSTANDARD1_0
using System.Collections.Concurrent;
#else
using System.Threading;
using System.Collections.Generic;
#endif

namespace Cyxor.Serialization
{
    partial class SerialStream
    {
        static partial class Reflector
        {
            internal sealed class ConcurrentCache<TKey, TValue>
#if NET35 || NETSTANDARD1_0
                : IDisposable
#endif
                where TKey : notnull
            {
#if NET20 || NET35 || NETSTANDARD1_0

#if !NET20
                readonly ReaderWriterLockSlim RwLock = new ReaderWriterLockSlim();

                public void Dispose()
                    => RwLock.Dispose();
#else
                readonly ReaderWriterLock RwLock = new ReaderWriterLock();
#endif
                readonly Dictionary<TKey, TValue> Items = new Dictionary<TKey, TValue>();

                public bool TryAdd(TKey key, TValue value)
                {
#if NET20
                    RwLock.AcquireWriterLock(Timeout.Infinite);
#else
                    RwLock.EnterWriteLock();
#endif
                    try
                    {
                        Items.Add(key, value);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                    finally
                    {
#if NET20
                        RwLock.ReleaseWriterLock();
#else
                        RwLock.ExitWriteLock();
#endif
                    }
                }

                public bool TryGetValue(TKey key, out TValue value)
                {
#if NET20
                    RwLock.AcquireReaderLock(Timeout.Infinite);
#else
                    RwLock.EnterReadLock();
#endif
                    try
                    {
                        return Items.TryGetValue(key, out value);
                    }
                    finally
                    {
#if NET20
                        RwLock.ReleaseReaderLock();
#else
                        RwLock.ExitReadLock();
#endif
                    }
                }
#else
                readonly ConcurrentDictionary<TKey, TValue> Items = new ConcurrentDictionary<TKey, TValue>();

                public bool TryAdd(TKey key, TValue value)
                    => Items.TryAdd(key, value);

                public bool TryGetValue(TKey key, out TValue value)
                    => Items.TryGetValue(key, out value);
#endif
            }
        }
    }
}