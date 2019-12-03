﻿#if NET20 || NET35 || NET40 || NET45 || NETSTANDARD1_0 || NETSTANDARD1_3 || NETSTANDARD2_0

using System;

namespace Cyxor.Extensions
{
    static class StringExtensions
    {
        public static int IndexOf(this string thisString, char value, StringComparison stringComparison)
            => thisString.IndexOf(new string(value, 1), stringComparison);

        public static bool Contains(this string thisString, string value, StringComparison stringComparison)
            => thisString.IndexOf(value, stringComparison) != -1;
    }
}
#endif