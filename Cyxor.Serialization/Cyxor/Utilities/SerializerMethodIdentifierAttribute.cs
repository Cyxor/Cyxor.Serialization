using System;

namespace Cyxor.Serialization
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    sealed class SerializerMethodIdentifierAttribute : Attribute
    {
        public readonly SerializerMethodIdentifier Identifier;

        public SerializerMethodIdentifierAttribute(SerializerMethodIdentifier identifier)
        {
            Identifier = identifier;
        }
    }
}
