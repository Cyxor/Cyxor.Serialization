using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization
{
    class NameEqualityComparer : IEqualityComparer<Type>
    {
        public static NameEqualityComparer Instance = new NameEqualityComparer();

        public bool Equals([AllowNull] Type x, [AllowNull] Type y)
            => x.Name == y.Name && x.FullName == y.FullName;

        public int GetHashCode([DisallowNull] Type obj)
            => HashCode.Combine(obj);
    }
}
