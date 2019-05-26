#if NETSTANDARD1_0 || NETSTANDARD1_3

using System.IO;

namespace Cyxor.Extensions
{
    static class StreamExtensions
    {
        public static byte[] GetBuffer(this MemoryStream value)
#if NETSTANDARD1_0
            => value.ToArray();
#else
        {
            value.TryGetBuffer(out var arraySegment);
            return arraySegment.Array;
        }
#endif
    }
}
#endif